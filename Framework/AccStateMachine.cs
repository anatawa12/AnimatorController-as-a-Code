using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class AccStateMachine : IACCStateMachine
    {
        private AccState _currentState;
        [CanBeNull] internal AccState LastState;
        internal readonly AnimatorStateMachine StateMachine;

        internal AccStateMachine(AnimatorStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public AccState NewState(string name)
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
            return _currentState = new AccState(this, animatorState).Under();
        }

        public AccEntryTransition EntryTransitionsTo(AccState state)
        {
            return new AccEntryTransition(StateMachine.AddEntryTransition(state.State), this);
        }

        public AccTransition AnyTransitionsTo(AccState state)
        {
            return new AccTransition(StateMachine.AddAnyStateTransition(state.State), this);
        }
    }

    interface IACCStateMachine
    {
        AccState NewState(string name);
        AccEntryTransition EntryTransitionsTo(AccState state);
        AccTransition AnyTransitionsTo(AccState state);
    }
}
