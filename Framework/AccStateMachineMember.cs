using JetBrains.Annotations;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class AccStateMachineMember
    {
        [NotNull] internal readonly AccStateMachineBase ParentMachine;
        internal Vector3 Position { get; private set; }
        private protected readonly AccConfig Config;

        private protected AccStateMachineMember([NotNull] AccStateMachineBase parentMachine, AccConfig config)
        {
            ParentMachine = parentMachine;
            Config = config;
        }

        internal abstract void SetTransitionTarget(AnimatorTransitionBase transition);

        //public abstract AccTransitionBase TransitionsTo(AccStateMachineMember target);

        #region position

        protected AccStateMachineMember LeftOf(AccStateMachineMember of = null) => Offset(of, -1, 0);
        protected AccStateMachineMember RightOf(AccStateMachineMember of = null) => Offset(of, 1, 0);
        protected AccStateMachineMember Over(AccStateMachineMember of = null) => Offset(of, 0, -1);
        protected AccStateMachineMember Under(AccStateMachineMember of = null) => Offset(of, 0, 1);

        protected AccStateMachineMember Offset(AccStateMachineMember of, float offsetX, float offsetY)
        {
            var position = of?.Position ?? ParentMachine.LastMember?.Position;
            if (position.HasValue)
                Position = position.Value + new Vector3(offsetX * Config.StateOffset.x, offsetY * Config.StateOffset.y, 0);
            else
                Position = Config.FirstStateAt * Config.StateOffset;
            return this;
        }

        protected AccStateMachineMember OnGridAt(float offsetX, float offsetY)
        {
            Position = new Vector3(offsetX * Config.StateOffset.x, offsetY * Config.StateOffset.y, 0);
            return this;
        }
        #endregion

    }
}
