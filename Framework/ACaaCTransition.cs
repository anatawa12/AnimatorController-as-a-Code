using UnityEditor.Animations;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class ACaaCTransitionBase<T>
        where T : AnimatorTransitionBase
    {
        private protected readonly T Transition;
        private protected readonly ACaaCStateMachine StateMachine;

        private protected ACaaCTransitionBase(T transition, ACaaCStateMachine stateMachine)
        {
            Transition = transition;
            StateMachine = stateMachine;
        }
    }

    public class ACaaCTransition : ACaaCTransitionBase<AnimatorStateTransition>
    {
        internal ACaaCTransition(AnimatorStateTransition transition, ACaaCStateMachine stateMachine) : base(transition, stateMachine)
        {
        }

        public ACaaCTransition WithTransitionDurationSeconds(float seconds)
        {
            Transition.duration = seconds;
            return this;
        }

        // TODO: continuation
        public void When(ACaaCParameterCondition condition)
        {
            condition.ApplyTo(Transition);
        }
    }

    public class ACaaCEntryTransition : ACaaCTransitionBase<AnimatorTransition>
    {
        public ACaaCEntryTransition(AnimatorTransition transition, ACaaCStateMachine stateMachine) : base(transition, stateMachine) {}
    }
}
