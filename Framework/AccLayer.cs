using System;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class AccLayer : IACCStateMachine, IACCParameterHolder
    {
        public readonly AnimatorControllerLayer Layer;
        private readonly AccStateMachine _machine;
        private readonly Acc _acc;

        internal AccLayer(AnimatorControllerLayer layer, Acc acc)
        {
            Layer = layer;
            _machine = new AccStateMachine(layer.stateMachine, acc.Config);
            _acc = acc;
        }

        public AccLayer WithMask(AvatarMask mask)
        {
            Layer.avatarMask = mask;
            return this;
        }

        #region IACCStateMachine delegateion
        public AccState NewState(string name) => _machine.NewState(name);
        public AccEntryTransition EntryTransitionsTo(AccState state) => _machine.EntryTransitionsTo(state);
        public AccTransition AnyTransitionsTo(AccState state) => _machine.AnyTransitionsTo(state);
        #endregion

        public AccParameter<float> FloatParameter(string name) => _acc.FloatParameter(name);
        public AccParameter<int> IntParameter(string name) => _acc.IntParameter(name);
        public AccParameter<bool> BoolParameter(string name) => _acc.BoolParameter(name);

        public AccParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            _acc.EnumParameter<T>(name);

    }
}
