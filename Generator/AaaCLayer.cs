using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorAsACode.Generator
{
    public sealed class AaaCLayer
    {
        private readonly AnimatorControllerLayer _layer;

        internal AaaCLayer(AnimatorControllerLayer layer)
        {
            _layer = layer;
        }

        public AaaCLayer WithMask(AvatarMask mask)
        {
            _layer.avatarMask = mask;
            return this;
        }
    }
}