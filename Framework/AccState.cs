using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public sealed class AccState
    {
        private readonly AccStateMachine _stateMachine;
        private readonly AccConfig _config;
        internal readonly AnimatorState State;

        private Vector3 Positon
        {
            get => _stateMachine.StateMachine.states.First(x => x.state == State).position;
            set
            {
                var states = _stateMachine.StateMachine.states;
                for (var i = 0; i < states.Length; i++)
                {
                    if (states[i].state == State)
                    {
                        var state = states[i];
                        state.position = value;
                        states[i] = state;
                        _stateMachine.StateMachine.states = states;
                        return;
                    }
                }

                throw new InvalidOperationException("Not found");
            }
        }

        public AccState([NotNull] AccStateMachine accStateMachine, AnimatorState state, AccConfig config)
        {
            _stateMachine = accStateMachine;
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
            (T) State.behaviours.FirstOrDefault(x => x.GetType() == typeof(T)) 
            ?? State.AddStateMachineBehaviour<T>();

        #region position

        public AccState LeftOf(AccState of = null) => Offset(of, -1, 0);
        public AccState RightOf(AccState of = null) => Offset(of, 1, 0);
        public AccState Over(AccState of = null) => Offset(of, 0, -1);
        public AccState Under(AccState of = null) => Offset(of, 0, 1);

        public AccState Offset(AccState of, int offsetX, int offsetY)
        {
            var position = of?.Positon ?? _stateMachine.LastState?.Positon;
            if (position.HasValue)
            {
                Positon = position.Value + new Vector3(offsetX * _config.StateOffset.x, offsetY * _config.StateOffset.y, 0);
            }
            else
            {
                Positon = _config.FirstStateAt;
            }
            return this;
        }
        #endregion

        public AccState MotionTime(AccParameter<float> weight)
        {
            State.timeParameter = weight.Name;
            State.timeParameterActive = true;
            return this;
        }

        public AccTransition TransitionsTo(AccState target) => new AccTransition(State.AddTransition(target.State), _stateMachine);
    }
}
