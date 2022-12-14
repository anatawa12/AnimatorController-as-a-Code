using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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

        public new AccStateMachine Offset(AccStateMachineMember of, float offsetX, float offsetY) =>
            (AccStateMachine)base.Offset(of, offsetX, offsetY);

        public new AccStateMachine OnGridAt(float offsetX, float offsetY) =>
            (AccStateMachine)base.OnGridAt(offsetX, offsetY);

        #endregion

        public AccStateMachineTransition TransitionsTo(AccStateMachineMember target)
        {
            if (ParentMachine != target.ParentMachine)
                throw new ArgumentException("parent StateMachine mismatch", nameof(target));
            var transition = ParentMachine.AddStateMachineTransition(this);
            target.SetTransitionTarget(transition.Transition);
            return transition;
        }

        public AccStateMachineTransition TransitionsToExit()
        {
            var transition = ParentMachine.AddStateMachineTransition(this);
            transition.Transition.isExit = true;
            return transition;
        }

        internal override void SetTransitionTarget(AnimatorTransitionBase transition) =>
            transition.destinationStateMachine = StateMachine;

        public void SaveToAsset()
        {
            _base.SaveToAsset();
        }
    }

    internal sealed class AccStateMachineBase : IAccStateMachine
    {
        private AccStateMachineMember _currentMember;
        private readonly AccConfig _config;
        [CanBeNull] internal AccStateMachineMember LastMember;
        internal readonly AnimatorStateMachine StateMachine;

        private readonly List<AccState> _addingStates = new List<AccState>();
        private readonly List<AccStateMachine> _addingStateMachines = new List<AccStateMachine>();

        private readonly List<AccEntryTransition> _addingEntryTransitions = new List<AccEntryTransition>();
        private readonly List<AccTransition> _addingAnyStateTransitions = new List<AccTransition>();

        private readonly Dictionary<AccStateMachine, List<AccStateMachineTransition>> _addingStateMachineTransitions =
            new Dictionary<AccStateMachine, List<AccStateMachineTransition>>();

        internal AccStateMachineBase(AnimatorStateMachine stateMachine, AccConfig config)
        {
            StateMachine = stateMachine;
            _config = config;
        }

        public AccState NewState(string name)
        {
            LastMember = _currentMember;
            var state = new AccState(this,
                new AnimatorState { hideFlags = HideFlags.HideInHierarchy, writeDefaultValues = false, name = name },
                _config).Under();
            Utils.AddToFile(StateMachine, state.State);
            _currentMember = state;
            _addingStates.Add(state);
            return state;
        }

        public AccStateMachine NewSubStateMachine(string name)
        {
            LastMember = _currentMember;
            var machine =
                new AccStateMachine(new AnimatorStateMachine { hideFlags = HideFlags.HideInHierarchy, name = name },
                    _config, this).Under();
            Utils.AddToFile(StateMachine, machine.StateMachine);
            _addingStateMachines.Add(machine);
            _currentMember = machine;
            return machine;
        }

        public AccEntryTransition EntryTransitionsTo(AccStateMachineMember state)
        {
            var transition = new AccEntryTransition(
                    new AnimatorTransition { hideFlags = HideFlags.HideInHierarchy }, this);
            state.SetTransitionTarget(transition.Transition);
            _addingEntryTransitions.Add(transition);
            Utils.AddToFile(StateMachine, transition.Transition);
            return transition;
        }

        public AccTransition AnyTransitionsTo(AccStateMachineMember state)
        {
            var transition = new AccTransition(
                new AnimatorStateTransition { hideFlags = HideFlags.HideInHierarchy }, this);
            state.SetTransitionTarget(transition.Transition);
            _addingAnyStateTransitions.Add(transition);
            Utils.AddToFile(StateMachine, transition.Transition);
            return transition;
        }

        internal AccStateMachineTransition AddStateMachineTransition(AccStateMachine sourceStateMachine)
        {
            if (!_addingStateMachineTransitions.TryGetValue(sourceStateMachine, out var list))
                _addingStateMachineTransitions[sourceStateMachine] = list = new List<AccStateMachineTransition>();
            var transition =
                new AccStateMachineTransition(new AnimatorTransition { hideFlags = HideFlags.HideInHierarchy }, this);
            list.Add(transition);
            Utils.AddToFile(StateMachine, transition.Transition);
            return transition;
        }

        public void SaveToAsset()
        {
            foreach (var accState in _addingStates) accState.SaveToAsset();
            foreach (var stateMachine in _addingStateMachines) stateMachine.SaveToAsset();
            foreach (var entryTransition in _addingEntryTransitions) entryTransition.SaveToAsset();
            foreach (var transition in _addingAnyStateTransitions) transition.SaveToAsset();
            foreach (var (sourceMachine, list) in _addingStateMachineTransitions)
            {
                var machineTransitions = StateMachine.GetStateMachineTransitions(sourceMachine.StateMachine);
                machineTransitions = Utils.JoinArray(machineTransitions, list, x => x.Transition);
                StateMachine.SetStateMachineTransitions(sourceMachine.StateMachine, machineTransitions);
                foreach (var transition in list) transition.SaveToAsset();
            }

            StateMachine.states = Utils.JoinArray(StateMachine.states, _addingStates,
                x => new ChildAnimatorState { state = x.State, position = x.Position });
            StateMachine.stateMachines = Utils.JoinArray(StateMachine.stateMachines, _addingStateMachines,
                x => new ChildAnimatorStateMachine { stateMachine = x.StateMachine, position = x.Position });
            StateMachine.entryTransitions = Utils.JoinArray(StateMachine.entryTransitions, 
                _addingEntryTransitions, x => x.Transition);
            StateMachine.anyStateTransitions = Utils.JoinArray(StateMachine.anyStateTransitions,
                _addingAnyStateTransitions, x => x.Transition);
            // _addingStateMachineTransitions has been added
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
