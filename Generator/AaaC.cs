using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorAsACode.Generator
{
    public sealed class AaaC
    {
        private readonly string _layerBaseName;
        private readonly AnimatorController _controller;

        internal AaaC(string layerBaseName, AnimatorController controller)
        {
            _layerBaseName = layerBaseName;
            _controller = controller;
        }

        public AaaCLayer AddMainLayer() => DoAddLayer(_layerBaseName);
        public AaaCLayer AddLayer(string name) => DoAddLayer($"{_layerBaseName}_{name}");

        private AaaCLayer DoAddLayer(string layerName)
        {
            var layer = new AnimatorControllerLayer
            {
                name = layerName,
                stateMachine = new AnimatorStateMachine
                {
                    name = layerName,
                    hideFlags = HideFlags.HideInHierarchy
                }
            };

            AssetDatabase.AddObjectToAsset(layer.stateMachine, _controller);
            _controller.AddLayer(layer);

            return new AaaCLayer(layer);
        }
    }
}