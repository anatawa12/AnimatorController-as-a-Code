using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public sealed class ACaaC : IACaaCParameterHolder
    {
        private readonly string _layerBaseName;
        private readonly AnimatorController _controller;

        internal ACaaC(string layerBaseName, AnimatorController controller)
        {
            _layerBaseName = layerBaseName;
            _controller = controller;
        }

        public ACaaCLayer AddMainLayer() => DoAddLayer(_layerBaseName);
        public ACaaCLayer AddLayer(string name) => DoAddLayer($"{_layerBaseName}_{name}");

        private ACaaCLayer DoAddLayer(string layerName)
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

            return new ACaaCLayer(layer, this);
        }

        public ACaaCParameter<float> FloatParameter(string name) => Parameter(name, AnimatorControllerParameterType.Float, Utils.FloatToFloat);
        public ACaaCParameter<int> IntParameter(string name) => Parameter(name, AnimatorControllerParameterType.Int, Utils.IntToFloat);
        public ACaaCParameter<bool> BoolParameter(string name) => Parameter(name, AnimatorControllerParameterType.Bool, Utils.BoolToFloat);

        public ACaaCParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            Parameter(name, AnimatorControllerParameterType.Int, Utils.EnumToFloat<T>());

        private ACaaCParameter<T> Parameter<T>(string name, AnimatorControllerParameterType type, Func<T, float> toFloat)
        {
            var found = _controller.parameters.FirstOrDefault(x => x.name == name);
            if (found != null)
            {
                if (found.type != type)
                    throw new InvalidOperationException(
                        $"Parameter named {name} found but type is {found.type} which is not expected type, {type}");
                return new ACaaCParameter<T>(found, toFloat);
            }

            // add
            var parameter = new AnimatorControllerParameter
            {
                name = name,
                type = type
            };
            _controller.AddParameter(parameter);
            return new ACaaCParameter<T>(parameter, toFloat);
        }

        public ACaaCClip NewClip()
        {
            var clip = new AnimationClip
            {
                hideFlags = HideFlags.HideInHierarchy
            };

            Utils.AddToFile(_controller, clip);

            return new ACaaCClip(clip);
        }
    }

    public interface IACaaCParameterHolder
    {
        ACaaCParameter<float> FloatParameter(string name);
        ACaaCParameter<int> IntParameter(string name);
        ACaaCParameter<bool> BoolParameter(string name);
        ACaaCParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum;
    }
}
