using JetBrains.Annotations;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class AccStateMachineMember
    {
        [NotNull] internal readonly AccStateMachineBase ParentMachine;
        private protected abstract Vector3 Positon { get; set; }
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
            var position = of?.Positon ?? ParentMachine.LastMember?.Positon;
            if (position.HasValue)
            {
                Positon = position.Value + new Vector3(offsetX * Config.StateOffset.x, offsetY * Config.StateOffset.y, 0);
            }
            else
            {
                Positon = Config.FirstStateAt * Config.StateOffset;
            }
            return this;
        }

        protected AccStateMachineMember OnGridAt(float offsetX, float offsetY)
        {
            Positon = new Vector3(offsetX * Config.StateOffset.x, offsetY * Config.StateOffset.y, 0);
            return this;
        }
        #endregion

    }
}
