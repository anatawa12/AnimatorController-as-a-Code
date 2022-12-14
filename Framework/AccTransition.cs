using UnityEditor.Animations;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class AccTransitionBase<T>
        where T : AnimatorTransitionBase
    {
        private protected readonly T Transition;
        private protected readonly AccStateMachineBase StateMachine;

        private protected AccTransitionBase(T transition, AccStateMachineBase stateMachine)
        {
            Transition = transition;
            StateMachine = stateMachine;
        }

        // TODO: continuation
        public void When(AccParameterCondition condition)
        {
            condition.ApplyTo(Transition);
        }
    }

    public class AccTransition : AccTransitionBase<AnimatorStateTransition>
    {
        internal AccTransition(AnimatorStateTransition transition, AccStateMachineBase stateMachine) : base(transition, stateMachine)
        {
        }

        public AccTransition WithTransitionDurationSeconds(float seconds)
        {
            Transition.duration = seconds;
            return this;
        }
    }

    public class AccStateMachineTransition : AccTransitionBase<AnimatorTransition>
    {
        internal AccStateMachineTransition(AnimatorTransition transition, AccStateMachineBase stateMachine) : base(transition, stateMachine)
        {
        }
    }

    public class AccEntryTransition : AccTransitionBase<AnimatorTransition>
    {
        internal AccEntryTransition(AnimatorTransition transition, AccStateMachineBase stateMachine) : base(transition, stateMachine) {}
    }
}
