using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public sealed class ACaaCLayer
    {
        private readonly AnimatorControllerLayer _layer;

        internal ACaaCLayer(AnimatorControllerLayer layer)
        {
            _layer = layer;
        }

        public ACaaCLayer WithMask(AvatarMask mask)
        {
            _layer.avatarMask = mask;
            return this;
        }
    }
}