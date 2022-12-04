using System;
using System.Linq;
using JetBrains.Annotations;
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