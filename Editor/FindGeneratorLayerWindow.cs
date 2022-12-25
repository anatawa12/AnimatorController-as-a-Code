using System;
using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    internal class FindGeneratorLayerWindow : EditorWindow
    {
        private AnimatorControllerGeneratorEditor _parentEditor;
        private string _searching;
        private Type[] _types;
        private Vector2 _scrollPosition = default;

        private void OnGUI()
        {
            if (_types == null)
                _types = SubclassHolder.Subclasses;
            EditorGUI.BeginChangeCheck();
            _searching = EditorGUILayout.TextField(_searching, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                _types = SearchTypes(_searching);
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            foreach (var type in _types)
            {
                if (GUILayout.Button(type.Name))
                {
                    _parentEditor.AddLayer(type);
                    Close();
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndScrollView();
        }

        private Type[] SearchTypes(string searching)
        {
            if (string.IsNullOrEmpty(searching))
                return SubclassHolder.Subclasses;
            return SubclassHolder.Subclasses
                .Where(x => x.Name.IndexOf(searching, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
        }

        public static void Show(Rect rect, AnimatorControllerGeneratorEditor parentEditor)
        {
            CloseAllOpenWindows();
            var window = CreateInstance<FindGeneratorLayerWindow>();
            window._parentEditor = parentEditor;
            window.Init(rect);
        }

        private void Init(Rect buttonRect)
        {         
            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
            ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, 300));
            wantsMouseMove = true;
        }

        private static void CloseAllOpenWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll(typeof(FindGeneratorLayerWindow));
            foreach (var window in windows)
            {
                try
                {
                    ((EditorWindow)window).Close();
                }
                catch
                {
                    DestroyImmediate(window);
                }
            }
        }
    }

    internal static class SubclassHolder
    {
        public static Type[] Subclasses = FindSubclasses();

        private static Type[] FindSubclasses()
        {
            var list = CompilationPipeline.GetAssemblies().SelectMany(a => Assembly.Load(a.name).ExportedTypes)
                .Where(t => t.IsSubclassOf(typeof(GeneratorLayerBase))).ToList();
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return list.ToArray();
        }

        static SubclassHolder()
        {
            CompilationPipeline.compilationFinished += _ => Subclasses = FindSubclasses();
        }
    }
}
