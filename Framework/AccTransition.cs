using UnityEditor.Animations;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class AccTransitionBase<T>
        where T : AnimatorTransitionBase
    {
        private protected readonly T Transition;
        private protected readonly AccStateMachine StateMachine;

        private protected AccTransitionBase(T transition, AccStateMachine stateMachine)
        {
            Transition = transition;
            StateMachine = stateMachine;
        }
    }

    public class AccTransition : AccTransitionBase<AnimatorStateTransition>
    {
        internal AccTransition(AnimatorStateTransition transition, AccStateMachine stateMachine) : base(transition, stateMachine)
        {
        }

        public AccTransition WithTransitionDurationSeconds(float seconds)
        {
            Transition.duration = seconds;
            return this;
        }

        // TODO: continuation
        public void When(AccParameterCondition condition)
        {
            condition.ApplyTo(Transition);
        }
    }

    public class AccEntryTransition : AccTransitionBase<AnimatorTransition>
    {
        public AccEntryTransition(AnimatorTransition transition, AccStateMachine stateMachine) : base(transition, stateMachine) {}
    }
}
