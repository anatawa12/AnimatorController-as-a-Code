using UnityEditor.Animations;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class ACCTransitionBase<T>
        where T : AnimatorTransitionBase
    {
        private protected readonly T Transition;
        private protected readonly ACCStateMachine StateMachine;

        private protected ACCTransitionBase(T transition, ACCStateMachine stateMachine)
        {
            Transition = transition;
            StateMachine = stateMachine;
        }
    }

    public class ACCTransition : ACCTransitionBase<AnimatorStateTransition>
    {
        internal ACCTransition(AnimatorStateTransition transition, ACCStateMachine stateMachine) : base(transition, stateMachine)
        {
        }

        public ACCTransition WithTransitionDurationSeconds(float seconds)
        {
            Transition.duration = seconds;
            return this;
        }

        // TODO: continuation
        public void When(ACCParameterCondition condition)
        {
            condition.ApplyTo(Transition);
        }
    }

    public class ACCEntryTransition : ACCTransitionBase<AnimatorTransition>
    {
        public ACCEntryTransition(AnimatorTransition transition, ACCStateMachine stateMachine) : base(transition, stateMachine) {}
    }
}
