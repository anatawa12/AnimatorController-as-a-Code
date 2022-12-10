using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class ACC : IACCParameterHolder
    {
        private readonly string _layerBaseName;
        public readonly AnimatorController Controller;

        internal ACC(string layerBaseName, AnimatorController controller)
        {
            _layerBaseName = layerBaseName;
            Controller = controller;
        }

        public ACCLayer AddMainLayer() => DoAddLayer(_layerBaseName);
        public ACCLayer AddLayer(string name) => DoAddLayer($"{_layerBaseName}_{name}");

        private ACCLayer DoAddLayer(string layerName)
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

            AssetDatabase.AddObjectToAsset(layer.stateMachine, Controller);
            Controller.AddLayer(layer);

            return new ACCLayer(layer, this);
        }

        public ACCParameter<float> FloatParameter(string name) => Parameter(name, AnimatorControllerParameterType.Float, Utils.FloatToFloat);
        public ACCParameter<int> IntParameter(string name) => Parameter(name, AnimatorControllerParameterType.Int, Utils.IntToFloat);
        public ACCParameter<bool> BoolParameter(string name) => Parameter(name, AnimatorControllerParameterType.Bool, Utils.BoolToFloat);

        public ACCParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            Parameter(name, AnimatorControllerParameterType.Int, Utils.EnumToFloat<T>());

        private ACCParameter<T> Parameter<T>(string name, AnimatorControllerParameterType type, Func<T, float> toFloat)
        {
            var found = Controller.parameters.FirstOrDefault(x => x.name == name);
            if (found != null)
            {
                if (found.type != type)
                    throw new InvalidOperationException(
                        $"Parameter named {name} found but type is {found.type} which is not expected type, {type}");
                return new ACCParameter<T>(found, toFloat);
            }

            // add
            var parameter = new AnimatorControllerParameter
            {
                name = name,
                type = type
            };
            Controller.AddParameter(parameter);
            return new ACCParameter<T>(parameter, toFloat);
        }

        public ACCClip NewClip()
        {
            var clip = new AnimationClip
            {
                hideFlags = HideFlags.HideInHierarchy
            };

            Utils.AddToFile(Controller, clip);

            return new ACCClip(clip);
        }
    }

    public interface IACCParameterHolder
    {
        ACCParameter<float> FloatParameter(string name);
        ACCParameter<int> IntParameter(string name);
        ACCParameter<bool> BoolParameter(string name);
        ACCParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum;
    }
}
