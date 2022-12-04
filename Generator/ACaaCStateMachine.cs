using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public sealed class ACaaCStateMachine : IACaaCStateMachine
    {
        private ACaaCState _currentState;
        [CanBeNull] internal ACaaCState LastState;
        internal readonly AnimatorStateMachine StateMachine;

        internal ACaaCStateMachine(AnimatorStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public ACaaCState NewState(string name)
        {
            var animatorState = new AnimatorState
            {
                hideFlags = HideFlags.HideInHierarchy,
                name = name
            };
            Utils.AddToFile(StateMachine, animatorState);
            var states = StateMachine.states;
            var childState = new ChildAnimatorState
            {
                state = animatorState,
                position = Vector3.zero
            };
            ArrayUtility.Add(ref states, childState);
            StateMachine.states = states;

            LastState = _currentState;
            return _currentState = new ACaaCState(this, animatorState).Under();
        }

        public ACaaCEntryTransition EntryTransitionsTo(ACaaCState state)
        {
            return new ACaaCEntryTransition(StateMachine.AddEntryTransition(state.State), this);
        }

        public ACaaCTransition AnyTransitionsTo(ACaaCState state)
        {
            return new ACaaCTransition(StateMachine.AddAnyStateTransition(state.State), this);
        }
    }

    interface IACaaCStateMachine
    {
        ACaaCState NewState(string name);
        ACaaCEntryTransition EntryTransitionsTo(ACaaCState state);
        ACaaCTransition AnyTransitionsTo(ACaaCState state);
    }
}