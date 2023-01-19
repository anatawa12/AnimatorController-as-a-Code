using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

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

            // Localization
            I18N.DrawLanguagePicker();

            // General information
            GUILayout.BeginHorizontal();
            GUILayout.Label(I18N.Tr("editor:generator-for"), EditorStyles.label);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(target.TargetResolved, typeof(AnimatorController), false);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(I18N.Tr("editor:generates-for"), EditorStyles.label);
            var selected = (Transform) EditorGUILayout.ObjectField(target.target, typeof(Transform), false);
            if (!selected)
                target.target = selected;
            else if (PrefabUtility.IsPartOfAnyPrefab(selected) 
                     && PrefabUtility.IsOutermostPrefabInstanceRoot(selected.gameObject))
                target.target = selected;
            GUILayout.EndHorizontal();
            if (!target.target)
            {
                GUIStyle style  = new GUIStyle();
                style.normal.textColor  = Color.yellow;
                style.focused.textColor = Color.yellow;
                style.wordWrap = true;
                GUILayout.Label(I18N.Tr("editor:no-target-object"), style);
            }

            var generators = target.generators;
            if (_editors?.Length != generators.Length)
                _editors = new UnityEditor.Editor[generators.Length];
            for (var i = 0; i < generators.Length; i++)
            {
                var generator = generators[i];
                Debug.Assert(generator != null, nameof(generator) + " != null");

                HorizontalLine(marginBottom: false);
                GUILayout.Label(generator.DefaultName, EditorStyles.label);

                EditorGUI.BeginChangeCheck();
                generator.name = EditorGUILayout.TextField("name", generator.name);
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(target);

                if (generator.name.Contains('_'))
                {
                    var style = new GUIStyle();
                    style.normal.textColor  = Color.yellow;
                    style.focused.textColor = Color.yellow;
                    GUILayout.Label(I18N.Tr("editor:layer-name-_"), style);
                }

                if (generator.name.Length == 0)
                {
                    var style = new GUIStyle();
                    style.normal.textColor  = Color.yellow;
                    style.focused.textColor = Color.yellow;
                    GUILayout.Label(I18N.Tr("editor:layer-name-empty"), style);
                }

                GUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUILayout.Button(I18N.Tr("editor:move-up")))
                    {
                        (generators[i], generators[i - 1]) = (generators[i - 1], generators[i]);
                        (_editors[i], _editors[i - 1]) = (_editors[i - 1], _editors[i]);
                        EditorUtility.SetDirty(target);
                    }
                using (new EditorGUI.DisabledScope(i + 1 == generators.Length))
                    if (GUILayout.Button(I18N.Tr("editor:move-down")))
                    {
                        (generators[i], generators[i + 1]) = (generators[i + 1], generators[i]);
                        (_editors[i], _editors[i + 1]) = (_editors[i + 1], _editors[i]);
                        EditorUtility.SetDirty(target);
                    }
                if (GUILayout.Button(I18N.Tr("editor:remove")))
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
                GUILayout.Label(I18N.Tr("editor:invalid-class"), style);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var content = new GUIContent(I18N.Tr("editor:add-generator"));
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
            
            if (GUILayout.Button(I18N.Tr("editor:manual-generate")))
            {
                target.DoGenerate();
            }

            if (GUILayout.Button(I18N.Tr("editor:regenerate-all")))
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
            generator.SetGenerator(target);
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

        private void HorizontalLine(bool marginTop = true, bool marginBottom = true)
        {
            const float margin = 17f / 2;
            var maxHeight = 1f;
            if (marginTop) maxHeight += margin;
            if (marginBottom) maxHeight += margin;

            var rect = GUILayoutUtility.GetRect(
                EditorGUIUtility.fieldWidth, float.MaxValue, 
                1, maxHeight, GUIStyle.none);
            if (marginTop && marginBottom)
                rect.y += rect.height / 2 - 0.5f;
            else if (marginTop)
                rect.y += rect.height - 1f;
            else if (marginBottom)
                rect.y += 0;
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}
