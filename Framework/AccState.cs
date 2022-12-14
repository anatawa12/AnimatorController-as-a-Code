using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class AccState : AccStateMachineMember
    {
        private readonly AccConfig _config;
        internal readonly AnimatorState State;

        private protected override Vector3 Positon
        {
            get => ParentMachine.StateMachine.states.First(x => x.state == State).position;
            set
            {
                var states = ParentMachine.StateMachine.states;
                for (var i = 0; i < states.Length; i++)
                {
                    if (states[i].state == State)
                    {
                        var state = states[i];
                        state.position = value;
                        states[i] = state;
                        ParentMachine.StateMachine.states = states;
                        return;
                    }
                }

                throw new InvalidOperationException("Not found");
            }
        }

        internal AccState([NotNull] AccStateMachineBase accParentMachine, AnimatorState state, AccConfig config) : base(accParentMachine, config)
        {
            _config = config;
            State = state;
        }

        public AccState WithAnimation(Motion motion)
        {
            State.motion = motion;
            return this;
        }

        public AccState WithAnimation(AccClip clip)
        {
            clip.Clip.name = State.name;
            EditorUtility.SetDirty(clip.Clip);
            State.motion = clip.Clip;
            return this;
        }

        /// <summary>
        /// Find or add StateMachineBehaviour with type T and run action with it.
        /// This may not add StateMachineBehaviour. To add new one, use  <see cref="WithNewBehaviour{T}"/>..
        /// </summary>
        /// <param name="action">The action run with the StateMachineBehaviour</param>
        /// <typeparam name="T">The type of StateMachineBehaviour</typeparam>
        /// <returns></returns>
        public AccState WithBehaviour<T>(Action<T> action)
            where T : StateMachineBehaviour
        {
            action(AddOrFindStateMachineBehaviour<T>());
            return this;
        }

        /// <summary>
        /// Add StateMachineBehaviour with type T and run action with it.
        /// This always not add StateMachineBehaviour. To reuse already added one, use <see cref="WithBehaviour{T}"/>.
        /// </summary>
        /// <param name="action">The action run with the StateMachineBehaviour</param>
        /// <typeparam name="T">The type of StateMachineBehaviour</typeparam>
        /// <returns></returns>
        public AccState WithNewBehaviour<T>(Action<T> action)
            where T : StateMachineBehaviour
        {
            action(State.AddStateMachineBehaviour<T>());
            return this;
        }

        public T AddOrFindStateMachineBehaviour<T>() where T : StateMachineBehaviour =>
            FindStateMachineBehaviour<T>(x => true) ?? AddStateMachineBehaviour<T>();

        public T AddStateMachineBehaviour<T>() where T : StateMachineBehaviour => State.AddStateMachineBehaviour<T>();

        public T FindStateMachineBehaviour<T>(Func<T, bool> selector) where T : StateMachineBehaviour =>
            (T) State.behaviours.FirstOrDefault(x => x.GetType() == typeof(T) && selector((T)x));

        internal override void SetTransitionTarget(AnimatorTransitionBase transition) => transition.destinationState = State;

        #region position
        public new AccState LeftOf(AccStateMachineMember of = null) => (AccState)base.LeftOf(of);
        public new AccState RightOf(AccStateMachineMember of = null) => (AccState)base.RightOf(of);
        public new AccState Over(AccStateMachineMember of = null) => (AccState)base.Over(of);
        public new AccState Under(AccStateMachineMember of = null) => (AccState)base.Under(of);
        public new AccState Offset(AccStateMachineMember of, float offsetX, float offsetY) => (AccState)base.Offset(of, offsetX, offsetY);
        public new AccState OnGridAt(float offsetX, float offsetY) => (AccState)base.OnGridAt(offsetX, offsetY);
        #endregion

        public AccState MotionTime(AccParameter<float> weight)
        {
            State.timeParameter = weight.Name;
            State.timeParameterActive = true;
            return this;
        }

        public AccTransition TransitionsTo(AccStateMachineMember target)
        {
            var transition = new AnimatorStateTransition
            {
                hasExitTime = false,
                hasFixedDuration = true,
                hideFlags = HideFlags.HideInHierarchy,
            };
            Utils.AddToFile(State, transition);
            target.SetTransitionTarget(transition);
            State.AddTransition(transition);
            return new AccTransition(transition, ParentMachine);
        }
    }
}
