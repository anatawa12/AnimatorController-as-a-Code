using Anatawa12.AnimatorControllerAsACode.Editor;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Examples
{
    public class MaskLayer : GeneratorLayerBase
    {
        public AvatarMask mask;

        // optional: GetType().Name by default
        protected override string GeneratorName => "MaskLayer";

        protected override void Generate(ACaaC acaac)
        {
            acaac.AddMainLayer().WithMask(mask);
        }
    }
}
