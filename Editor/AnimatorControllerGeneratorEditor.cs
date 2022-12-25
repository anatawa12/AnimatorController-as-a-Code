using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    [CustomEditor(typeof(AnimatorControllerGenerator))]
    public class AnimatorControllerGeneratorEditor : UnityEditor.Editor
    {
        private bool _inspectorToggle;
        private bool _openingSelector;
        private Vector2 _selectorScroll = Vector2.zero;
        private MonoScript _script;
        private UnityEditor.Editor[] _editors;

        private void OnDestroy() => AssetDatabase.SaveAssets();

        public override void OnInspectorGUI()
        {
            // ReSharper disable once LocalVariableHidesMember
            var target = (AnimatorControllerGenerator)this.target;

            // clear nulls
            // TODO: instead clearing, keep & warn
            if (target.generators.Any(x => !x))
            {
                target.generators = target.generators.Where(x => x).ToArray();
                EditorUtility.SetDirty(target);
            }

            // General information
            GUILayout.BeginHorizontal();
            GUILayout.Label("Generator for ", EditorStyles.label);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(target.TargetResolved, typeof(AnimatorController), false);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Generates for ", EditorStyles.label);
            target.target = (Transform) EditorGUILayout.ObjectField(target.target, typeof(Transform), false);
            GUILayout.EndHorizontal();
            if (!target.target)
            {
                GUIStyle style  = new GUIStyle();
                style.normal.textColor  = Color.yellow;
                style.focused.textColor = Color.yellow;
                GUILayout.Label("Generate target GameObject is not specified! this may produce broken animation", style);
            }

            var generators = target.generators;
            if (_editors?.Length != generators.Length)
                _editors = new UnityEditor.Editor[generators.Length];
            for (var i = 0; i < generators.Length; i++)
            {
                var generator = generators[i];
                GUILayout.BeginHorizontal();
                // removed above
                // ReSharper disable once PossibleNullReferenceException
                GUILayout.Label(generator.DefaultName, EditorStyles.label);
                HorizontalLine();
                GUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                generator.name = EditorGUILayout.TextField("name", generator.name);
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);

                if (generator.name.Contains('_'))
                {
                    var style = new GUIStyle();
                    style.normal.textColor  = Color.yellow;
                    style.focused.textColor = Color.yellow;
                    GUILayout.Label("Name should not contain '_'!", style);
                }

                if (generator.name.Length == 0)
                {
                    var style = new GUIStyle();
                    style.normal.textColor  = Color.yellow;
                    style.focused.textColor = Color.yellow;
                    GUILayout.Label("Name should not be empty!", style);
                }

                GUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUILayout.Button("Move UP"))
                    {
                        (generators[i], generators[i - 1]) = (generators[i - 1], generators[i]);
                        (_editors[i], _editors[i - 1]) = (_editors[i - 1], _editors[i]);
                        EditorUtility.SetDirty(target);
                    }
                using (new EditorGUI.DisabledScope(i + 1 == generators.Length))
                    if (GUILayout.Button("Move Down"))
                    {
                        (generators[i], generators[i + 1]) = (generators[i + 1], generators[i]);
                        (_editors[i], _editors[i + 1]) = (_editors[i + 1], _editors[i]);
                        EditorUtility.SetDirty(target);
                    }
                if (GUILayout.Button("Remove"))
                {
                    ArrayUtility.RemoveAt(ref target.generators, i);
                    ArrayUtility.RemoveAt(ref _editors, i);
                    DestroyImmediate(generator, true);
                    generators = target.generators;
                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();

                CreateCachedEditor(generator, null, ref _editors[i]);
                _editors[i]?.OnInspectorGUI();
            }
                
            HorizontalLine();

            _script = (MonoScript)EditorGUILayout.ObjectField(_script, typeof(MonoScript), false);
            var validScriptClass = _script && (_script.GetClass()?.IsSubclassOf(typeof(GeneratorLayerBase)) ?? false);

            if (_script && !validScriptClass)
            {
                GUIStyle style  = new GUIStyle();
                style.normal.textColor  = Color.red;
                style.focused.textColor = Color.red;
                GUILayout.Label("The Class is not subclass of ControllerGeneratorBase.", style);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var content = new GUIContent("Add Generator");
            GUIStyle guiStyle = "AC Button";
            var rect = GUILayoutUtility.GetRect(content, guiStyle);
            if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, guiStyle))
            {
                if (_script != null)
                {
                    if (validScriptClass)
                        AddLayer(_script.GetClass());
                }
                else
                {
                    FindGeneratorLayerWindow.Show(rect, this);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            HorizontalLine();
            
            if (GUILayout.Button("Manual Generate"))
            {
                target.DoGenerate();
            }

            if (GUILayout.Button("Regenerate All"))
            {
                AutomaticGenerationCaller.RegenerateAll();
            }

            HorizontalLine();
            
#if ANATAWA12_ACC_DEBUG_INSPECTOR
            _inspectorToggle = EditorGUILayout.Foldout(_inspectorToggle, "DefaultInspectorForDebug");
            if (_inspectorToggle)
            {
                base.OnInspectorGUI();
            }
#endif
        }

        public void AddLayer(Type type)
        {
            // ReSharper disable once LocalVariableHidesMember
            var target = (AnimatorControllerGenerator)this.target;
            var generator = (GeneratorLayerBase)CreateInstance(type);
            generator.name = generator.DefaultName;
            if (target.generators.Any(x => x != null && x.name == generator.name))
            {
                var append = 1;
                while (target.generators.Any(x => x != null && x.name == $"{generator.name}{append}")) append++;
                generator.name = $"{generator.name}{append}";
            }

            AssetDatabase.AddObjectToAsset(generator, target);
            ArrayUtility.Add(ref target.generators, generator);
            ArrayUtility.Add(ref _editors, null);
            EditorUtility.SetDirty(target);
        }

        private void HorizontalLine()
        {
            var rect = GUILayoutUtility.GetRect(
                EditorGUIUtility.fieldWidth, float.MaxValue, 
                1, 18f, GUIStyle.none);
            rect.y += rect.height / 2 - 0.5f;
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}
