using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class Acc : IACCParameterHolder
    {
        private readonly string _layerBaseName;
        public readonly AnimatorController Controller;

        internal Acc(string layerBaseName, AnimatorController controller)
        {
            _layerBaseName = layerBaseName;
            Controller = controller;
        }

        public AccLayer AddMainLayer() => DoAddLayer(_layerBaseName);
        public AccLayer AddLayer(string name) => DoAddLayer($"{_layerBaseName}_{name}");

        private AccLayer DoAddLayer(string layerName)
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

            return new AccLayer(layer, this);
        }

        public AccParameter<float> FloatParameter(string name) => Parameter(name, AnimatorControllerParameterType.Float, Utils.FloatToFloat);
        public AccParameter<int> IntParameter(string name) => Parameter(name, AnimatorControllerParameterType.Int, Utils.IntToFloat);
        public AccParameter<bool> BoolParameter(string name) => Parameter(name, AnimatorControllerParameterType.Bool, Utils.BoolToFloat);

        public AccParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            Parameter(name, AnimatorControllerParameterType.Int, Utils.EnumToFloat<T>());

        private AccParameter<T> Parameter<T>(string name, AnimatorControllerParameterType type, Func<T, float> toFloat)
        {
            var found = Controller.parameters.FirstOrDefault(x => x.name == name);
            if (found != null)
            {
                if (found.type != type)
                    throw new InvalidOperationException(
                        $"Parameter named {name} found but type is {found.type} which is not expected type, {type}");
                return new AccParameter<T>(found, toFloat);
            }

            // add
            var parameter = new AnimatorControllerParameter
            {
                name = name,
                type = type
            };
            Controller.AddParameter(parameter);
            return new AccParameter<T>(parameter, toFloat);
        }

        public AccClip NewClip()
        {
            var clip = new AnimationClip
            {
                hideFlags = HideFlags.HideInHierarchy
            };

            Utils.AddToFile(Controller, clip);

            return new AccClip(clip);
        }
    }

    public interface IACCParameterHolder
    {
        AccParameter<float> FloatParameter(string name);
        AccParameter<int> IntParameter(string name);
        AccParameter<bool> BoolParameter(string name);
        AccParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum;
    }
}
