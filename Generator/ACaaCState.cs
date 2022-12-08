using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public sealed class ACaaCState
    {
        private static readonly Vector2 Grid = new Vector2(250, 70);
        private readonly ACaaCStateMachine _stateMachine;
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

        public ACaaCState([NotNull] ACaaCStateMachine aCaaCStateMachine, AnimatorState state)
        {
            _stateMachine = aCaaCStateMachine;
            State = state;
        }

        public ACaaCState WithAnimation(Motion motion)
        {
            State.motion = motion;
            return this;
        }

        public ACaaCState WithAnimation(ACaaCClip clip)
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
        public ACaaCState WithBehaviour<T>(Action<T> action)
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
        public ACaaCState WithNewBehaviour<T>(Action<T> action)
            where T : StateMachineBehaviour
        {
            action(State.AddStateMachineBehaviour<T>());
            return this;
        }

        public T AddOrFindStateMachineBehaviour<T>() where T : StateMachineBehaviour =>
            (T) State.behaviours.FirstOrDefault(x => x.GetType() == typeof(T)) 
            ?? State.AddStateMachineBehaviour<T>();

        #region position

        public ACaaCState LeftOf(ACaaCState of = null) => Offset(of, -1, 0);
        public ACaaCState RightOf(ACaaCState of = null) => Offset(of, 1, 0);
        public ACaaCState Over(ACaaCState of = null) => Offset(of, 0, -1);
        public ACaaCState Under(ACaaCState of = null) => Offset(of, 0, 1);

        public ACaaCState Offset(ACaaCState of, int offsetX, int offsetY)
        {
            var position = of?.Positon ?? _stateMachine.LastState?.Positon ?? Vector3.zero;
            Debug.Log($"_stateMachine.LastState: {_stateMachine.LastState?.Positon}");
            Positon = position + new Vector3(offsetX * Grid.x, offsetY * Grid.y, 0);
            return this;
        }
        #endregion

        public ACaaCState MotionTime(ACaaCParameter<float> weight)
        {
            State.timeParameter = weight.Name;
            State.timeParameterActive = true;
            return this;
        }

        public ACaaCTransition TransitionsTo(ACaaCState target) => new ACaaCTransition(State.AddTransition(target.State), _stateMachine);
    }
}
