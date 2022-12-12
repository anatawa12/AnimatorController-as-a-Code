using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class ACCStateMachine : IACCStateMachine
    {
        private ACCState _currentState;
        [CanBeNull] internal ACCState LastState;
        internal readonly AnimatorStateMachine StateMachine;

        internal ACCStateMachine(AnimatorStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public ACCState NewState(string name)
        {
            var animatorState = new AnimatorState
            {
                hideFlags = HideFlags.HideInHierarchy,
                writeDefaultValues = false,
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
            return _currentState = new ACCState(this, animatorState).Under();
        }

        public ACCEntryTransition EntryTransitionsTo(ACCState state)
        {
            return new ACCEntryTransition(StateMachine.AddEntryTransition(state.State), this);
        }

        public ACCTransition AnyTransitionsTo(ACCState state)
        {
            return new ACCTransition(StateMachine.AddAnyStateTransition(state.State), this);
        }
    }

    interface IACCStateMachine
    {
        ACCState NewState(string name);
        ACCEntryTransition EntryTransitionsTo(ACCState state);
        ACCTransition AnyTransitionsTo(ACCState state);
    }
}
