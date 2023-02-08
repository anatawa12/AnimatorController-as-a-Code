using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class Acc : IACCParameterHolder
    {
        private readonly string _layerBaseName;
        internal readonly AccConfig Config;
        public readonly AnimatorController Controller;
        private readonly List<AccLayer> _addingLayers = new List<AccLayer>();

        internal Acc(string layerBaseName, AnimatorController controller, AccConfig config)
        {
            _layerBaseName = layerBaseName;
            Config = config;
            Controller = controller;
        }

        public AccLayer AddMainLayer() => DoAddLayer(_layerBaseName);
        public AccLayer AddLayer(string name) => DoAddLayer($"{_layerBaseName}_{name}");

        private AccLayer DoAddLayer(string layerName)
        {
            var layer = new AccLayer(new AnimatorControllerLayer
            {
                name = layerName,
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine
                {
                    name = layerName,
                    hideFlags = HideFlags.HideInHierarchy
                }
            }, this);
            _addingLayers.Add(layer);
            Utils.AddToFile(Controller, layer.Layer.stateMachine);

            return layer;
        }

        public AccParameter<float> FloatParameter(string name) => Parameter(name, AnimatorControllerParameterType.Float, Utils.FloatToFloat);
        public AccParameter<int> IntParameter(string name) => Parameter(name, AnimatorControllerParameterType.Int, Utils.IntToFloat);
        public AccParameter<bool> BoolParameter(string name) => Parameter(name, AnimatorControllerParameterType.Bool, Utils.BoolToFloat);

        public AccParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            Parameter(name, AnimatorControllerParameterType.Int, Utils.EnumToFloat<T>());

        private AccParameter<T> Parameter<T>(string name, AnimatorControllerParameterType type, Func<T, float> toFloat)
            where T : unmanaged
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

            return new AccClip(clip, Config);
        }

        public void SaveToAsset()
        {
            foreach (var layer in _addingLayers) layer.SaveToAsset();
            Controller.layers = Utils.JoinArray(Controller.layers, _addingLayers, x => x.Layer);
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
