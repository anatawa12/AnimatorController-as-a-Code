using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Examples
{
    public class CopyFromOtherControllerLayer : GeneratorLayerBase
    {
        public AnimatorController controller;
        public string[] layerNames;

        protected override IEnumerable<Object> WatchingObjects => new[] { controller };

        protected override void Generate(Acc acc)
        {
            foreach (var layerName in layerNames)
            {
                var layer = controller.layers.FirstOrDefault(x => x.name == layerName);
                if (layer == null)
                {
                    Debug.LogWarning($"Layer with name {layerName} not found. skipping copying");
                    continue;
                }

                var newLayer = acc.AddLayer(layer.name);
                newLayer.Layer.avatarMask = layer.avatarMask;
                // m_Motions and m_Behaviours are copied via CopyOverrides
                newLayer.Layer.blendingMode = layer.blendingMode;
                newLayer.Layer.syncedLayerIndex = layer.syncedLayerIndex;
                newLayer.Layer.iKPass = layer.iKPass;
                newLayer.Layer.defaultWeight = layer.defaultWeight;
                newLayer.Layer.syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming;
                CopyOverrides(layer, newLayer.Layer, layer.stateMachine);                
            }
        }

        private void CopyOverrides(AnimatorControllerLayer oldLayer, AnimatorControllerLayer newLayer, AnimatorStateMachine stateMachine)
        {
            foreach (var state in stateMachine.states)
            {
                var behaviours = oldLayer.GetOverrideBehaviours(state.state);
                if (behaviours.Length != 0)
                    newLayer.SetOverrideBehaviours(state.state, behaviours);
                var motion = oldLayer.GetOverrideMotion(state.state);
                if (motion)
                    newLayer.SetOverrideMotion(state.state, motion);
            }

            foreach (var childStateMachine in stateMachine.stateMachines)
                CopyOverrides(oldLayer, newLayer, childStateMachine.stateMachine);
        }
    }

    [CustomEditor(typeof(CopyFromOtherControllerLayer))]
    public class OtherControllerLayerEditor : Editor
    {
        private int _addCandidate;

        public override void OnInspectorGUI()
        {
            var layer = (CopyFromOtherControllerLayer)target;


            layer.controller = (AnimatorController)EditorGUILayout.ObjectField("controller", layer.controller, typeof(AnimatorController), false);

            if (layer.layerNames == null)
                layer.layerNames = Array.Empty<string>();

            var swaps = (0, 0);
            var deletes = -1;

            var modified = false;

            var addableNames = layer.controller == null
                ? Array.Empty<string>()
                : layer.controller.layers.Select(x => x.name).Where(x => !layer.layerNames.Contains(x)).ToArray();

            using (new EditorGUI.DisabledScope(layer.controller == null))
            {
                for (var i = 0; i < layer.layerNames.Length; i++)
                {
                    GUILayout.BeginHorizontal();

                    var candidates = AddedFirst(layer.layerNames[i], addableNames);

                    var foundIndex = EditorGUILayout.Popup(0, candidates);
                    if (foundIndex != 0)
                    {
                        modified = true;
                        layer.layerNames[i] = candidates[foundIndex];
                    }

                    using (new EditorGUI.DisabledScope(i == 0))
                        if (GUILayout.Button("▲", EditorStyles.miniButton))
                            swaps = (i, i - 1);
                    using (new EditorGUI.DisabledScope(i == layer.layerNames.Length - 1))
                        if (GUILayout.Button("▼", EditorStyles.miniButton))
                            swaps = (i, i + 1);

                    if (GUILayout.Button("X", EditorStyles.miniButton))
                        deletes = i;

                    GUILayout.EndHorizontal();
                }

                using (new EditorGUI.DisabledScope(addableNames.Length == 0))
                {
                    if (GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        ArrayUtility.Add(ref layer.layerNames, addableNames[0]);
                        modified = true;
                    }

                    if (GUILayout.Button("Add All Missing Layers", EditorStyles.miniButton))
                    {
                        foreach (var layerName in addableNames)
                        {
                            ArrayUtility.Add(ref layer.layerNames, layerName);
                            modified = true;
                        }
                    }
                }

                if (addableNames.Length == 0)
                {
                    var style = new GUIStyle(EditorStyles.label);
                    style.normal.textColor = Color.red;
                    GUILayout.Label("The All layers are added.", style);
                }
            }

            if (swaps != (0, 0))
            {
                (layer.layerNames[swaps.Item2], layer.layerNames[swaps.Item1]) = (layer.layerNames[swaps.Item1], layer.layerNames[swaps.Item2]);
                modified = true;
            }
            else if (deletes != -1)
            {
                ArrayUtility.RemoveAt(ref layer.layerNames, deletes);
                modified = true;
            }

            if (modified)
                EditorUtility.SetDirty(layer);
        }

        private static T[] AddedFirst<T>(T value, T[] array)
        {
            var result = new T[array.Length + 1];
            result[0] = value;
            Array.Copy(array, 0, result, 1, array.Length);
            return result;
        }
    }
}
