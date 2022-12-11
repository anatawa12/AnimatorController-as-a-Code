using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    [CustomEditor(typeof(AnimatorControllerGenerator))]
    public class AnimatorControllerGeneratorEditor : UnityEditor.Editor
    {
        private bool _inspectorToggle;
        private bool _openingSelector;
        private Vector2 _selectorScroll = Vector2.zero;
        private MonoScript _script;

        // TODO: this will be called when name is specified and will case exception "created controller cannot be loaded"
        private void OnDestroy() => ((AnimatorControllerGenerator)target).DoGenerate();

        public override void OnInspectorGUI()
        {
            // ReSharper disable once LocalVariableHidesMember
            var target = (AnimatorControllerGenerator)this.target;

            // clear nulls
            if (target.generators.Any(x => !x))
            {
                target.generators = target.generators.Where(x => x).ToArray();
                EditorUtility.SetDirty(target);
            }

            var generators = target.generators;
            for (var i = 0; i < generators.Length; i++)
            {
                var generator = generators[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label(generator.GeneratorName, EditorStyles.label);
                HorizontalLine();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i == 0))
                    if (GUILayout.Button("Move UP"))
                    {
                        (generators[i], generators[i - 1]) = (generators[i - 1], generators[i]);
                        EditorUtility.SetDirty(target);
                    }
                using (new EditorGUI.DisabledScope(i + 1 == generators.Length))
                    if (GUILayout.Button("Move Down"))
                    {
                        (generators[i], generators[i + 1]) = (generators[i + 1], generators[i]);
                        EditorUtility.SetDirty(target);
                    }
                if (GUILayout.Button("Remove"))
                {
                    ArrayUtility.RemoveAt(ref target.generators, i);
                    DestroyImmediate(generator, true);
                    generators = target.generators;
                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();

                var editor = CreateEditor(generator);
                editor.OnInspectorGUI();
            }
                
            HorizontalLine();
            //DrawChildObject(serializedObject);
            
            bool validScriptClass;
            GUILayout.BeginHorizontal();
            {
                _script = (MonoScript)EditorGUILayout.ObjectField(_script, typeof(MonoScript), false);
                validScriptClass = _script && (_script.GetClass()?.IsSubclassOf(typeof(GeneratorLayerBase)) ?? false);
                
                using (new EditorGUI.DisabledScope(!validScriptClass))
                {
                    if (GUILayout.Button("Add Generator"))
                    {
                        var generator = (GeneratorLayerBase)Activator.CreateInstance(_script.GetClass());
                        AssetDatabase.AddObjectToAsset(generator, target);
                        ArrayUtility.Add(ref target.generators, generator);
                    }
                }
            }
            GUILayout.EndHorizontal();
            if (_script && !validScriptClass)
            {
                GUIStyle style  = new GUIStyle();
                style.normal.textColor  = Color.red;
                style.focused.textColor = Color.red;
                GUILayout.Label("The Class is not subclass of ControllerGeneratorBase.", style);
            }
            
            if (GUILayout.Button("Do Generate"))
            {
                target.DoGenerate();
            }

            HorizontalLine();
            
            _inspectorToggle = EditorGUILayout.Foldout(_inspectorToggle, "DefaultInspectorForDebug");
            if (_inspectorToggle)
            {
                base.OnInspectorGUI();
            }
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

        internal static class SubclassHolder
        {
            public static List<Type> TestAssetSubclasses = FindSubclasses();

            private static List<Type> FindSubclasses() =>
                CompilationPipeline.GetAssemblies().SelectMany(a => Assembly.Load(a.name).ExportedTypes)
                    .Where(t => t.IsSubclassOf(typeof(GeneratorLayerBase))).ToList();

            static SubclassHolder()
            {
                CompilationPipeline.compilationFinished += _ => TestAssetSubclasses = FindSubclasses();
            }
        }
    }
}
