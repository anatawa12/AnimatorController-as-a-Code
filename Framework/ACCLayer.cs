using System;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class ACCLayer : IACCStateMachine, IACCParameterHolder
    {
        public readonly AnimatorControllerLayer Layer;
        private readonly ACCStateMachine _machine;
        private readonly ACC _acc;

        internal ACCLayer(AnimatorControllerLayer layer, ACC acc)
        {
            Layer = layer;
            _machine = new ACCStateMachine(layer.stateMachine);
            _acc = acc;
        }

        public ACCLayer WithMask(AvatarMask mask)
        {
            Layer.avatarMask = mask;
            return this;
        }

        #region IACCStateMachine delegateion
        public ACCState NewState(string name) => _machine.NewState(name);
        public ACCEntryTransition EntryTransitionsTo(ACCState state) => _machine.EntryTransitionsTo(state);
        public ACCTransition AnyTransitionsTo(ACCState state) => _machine.AnyTransitionsTo(state);
        #endregion

        public ACCParameter<float> FloatParameter(string name) => _acc.FloatParameter(name);
        public ACCParameter<int> IntParameter(string name) => _acc.IntParameter(name);
        public ACCParameter<bool> BoolParameter(string name) => _acc.BoolParameter(name);

        public ACCParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            _acc.EnumParameter<T>(name);

    }
}
