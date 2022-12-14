using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class AccStateMachine : AccStateMachineMember, IAccStateMachine
    {
        AccStateMachineBase _base;
        internal AccStateMachine(AnimatorStateMachine stateMachine, AccConfig config, AccStateMachineBase parent)
            : base(parent, config)
        {
            _base = new AccStateMachineBase(stateMachine, config);
        }

        public AnimatorStateMachine StateMachine => _base.StateMachine;
        
        #region IACCStateMachine delegateion
        public AccState NewState(string name) => _base.NewState(name);
        public AccStateMachine NewSubStateMachine(string name) => _base.NewSubStateMachine(name);
        public AccEntryTransition EntryTransitionsTo(AccStateMachineMember state) => _base.EntryTransitionsTo(state);
        public AccTransition AnyTransitionsTo(AccStateMachineMember state) => _base.AnyTransitionsTo(state);
        #endregion
 
        #region position
        public new AccStateMachine LeftOf(AccStateMachineMember of = null) => (AccStateMachine)base.LeftOf(of);
        public new AccStateMachine RightOf(AccStateMachineMember of = null) => (AccStateMachine)base.RightOf(of);
        public new AccStateMachine Over(AccStateMachineMember of = null) => (AccStateMachine)base.Over(of);
        public new AccStateMachine Under(AccStateMachineMember of = null) => (AccStateMachine)base.Under(of);
        public new AccStateMachine Offset(AccStateMachineMember of, float offsetX, float offsetY) => (AccStateMachine)base.Offset(of, offsetX, offsetY);
        #endregion

        public AccStateMachineTransition TransitionsTo(AccStateMachineMember target)
        {
            if (ParentMachine != target.ParentMachine)
                throw new ArgumentException("parent StateMachine mismatch", nameof(target));
            var transition = ParentMachine.StateMachine.AddStateMachineTransition(StateMachine);
            target.SetTransitionTarget(transition);
            return new AccStateMachineTransition(transition, ParentMachine);
        }

        private protected override Vector3 Positon
        {
            get => ParentMachine.StateMachine.stateMachines.First(x => x.stateMachine == StateMachine).position;
            set
            {
                var stateMachines = ParentMachine.StateMachine.stateMachines;
                for (var i = 0; i < stateMachines.Length; i++)
                {
                    if (stateMachines[i].stateMachine == StateMachine)
                    {
                        var state = stateMachines[i];
                        state.position = value;
                        stateMachines[i] = state;
                        ParentMachine.StateMachine.stateMachines = stateMachines;
                        return;
                    }
                }

                throw new InvalidOperationException("Not found");
            }
        }


        internal override void SetTransitionTarget(AnimatorTransitionBase transition) =>
            transition.destinationStateMachine = StateMachine;
    }

    internal sealed class AccStateMachineBase : IAccStateMachine
    {
        private AccStateMachineMember _currentMember;
        private readonly AccConfig _config;
        [CanBeNull] internal AccStateMachineMember LastMember;
        internal readonly AnimatorStateMachine StateMachine;

        internal AccStateMachineBase(AnimatorStateMachine stateMachine, AccConfig config)
        {
            StateMachine = stateMachine;
            _config = config;
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

            var state = new AccState(this, animatorState, _config).Under();
            LastMember = _currentMember;
            _currentMember = state;
            return state;
        }

        public AccStateMachine NewSubStateMachine(string name)
        {
            var animatorStateMachine = new AnimatorStateMachine
            {
                hideFlags = HideFlags.HideInHierarchy,
                name = name
            };
            StateMachine.AddStateMachine(animatorStateMachine, Vector3.zero);
            Utils.AddToFile(StateMachine, animatorStateMachine);
            return new AccStateMachine(animatorStateMachine, _config, this);
        }

        public AccEntryTransition EntryTransitionsTo(AccStateMachineMember state)
        {
            var transition = new AnimatorTransition();
            Utils.AddToFile(StateMachine, transition);
            transition.hideFlags = HideFlags.HideInHierarchy;
            state.SetTransitionTarget(transition);

            var entryTransitions = StateMachine.entryTransitions;
            ArrayUtility.Add(ref entryTransitions, transition);
            StateMachine.entryTransitions = entryTransitions;

            return new AccEntryTransition(transition, this);
        }

        public AccTransition AnyTransitionsTo(AccStateMachineMember state)
        {
            var transition = new AnimatorStateTransition();
            Utils.AddToFile(StateMachine, transition);
            transition.hideFlags = HideFlags.HideInHierarchy;
            state.SetTransitionTarget(transition);

            var anyStateTransitions = StateMachine.anyStateTransitions;
            ArrayUtility.Add(ref anyStateTransitions, transition);
            StateMachine.anyStateTransitions = anyStateTransitions;

            return new AccTransition(transition, this);
        }
    }

    internal interface IAccStateMachine
    {
        AccState NewState(string name);
        AccStateMachine NewSubStateMachine(string name);
        AccEntryTransition EntryTransitionsTo(AccStateMachineMember state);
        AccTransition AnyTransitionsTo(AccStateMachineMember state);
    }
}
