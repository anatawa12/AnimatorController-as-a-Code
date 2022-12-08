using System;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class ACaaCLayer : IACaaCStateMachine, IACaaCParameterHolder
    {
        public readonly AnimatorControllerLayer Layer;
        private readonly ACaaCStateMachine _machine;
        private readonly ACaaC _acaac;

        internal ACaaCLayer(AnimatorControllerLayer layer, ACaaC acaac)
        {
            Layer = layer;
            _machine = new ACaaCStateMachine(layer.stateMachine);
            _acaac = acaac;
        }

        public ACaaCLayer WithMask(AvatarMask mask)
        {
            Layer.avatarMask = mask;
            return this;
        }

        #region IACaaCStateMachine delegateion
        public ACaaCState NewState(string name) => _machine.NewState(name);
        public ACaaCEntryTransition EntryTransitionsTo(ACaaCState state) => _machine.EntryTransitionsTo(state);
        public ACaaCTransition AnyTransitionsTo(ACaaCState state) => _machine.AnyTransitionsTo(state);
        #endregion

        public ACaaCParameter<float> FloatParameter(string name) => _acaac.FloatParameter(name);
        public ACaaCParameter<int> IntParameter(string name) => _acaac.IntParameter(name);
        public ACaaCParameter<bool> BoolParameter(string name) => _acaac.BoolParameter(name);

        public ACaaCParameter<T> EnumParameter<T>(string name)
            where T : unmanaged, Enum =>
            _acaac.EnumParameter<T>(name);

    }
}
