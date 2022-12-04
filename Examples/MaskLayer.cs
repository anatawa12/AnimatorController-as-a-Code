using Anatawa12.AnimatorControllerAsACode.Editor;
using Anatawa12.AnimatorControllerAsACode.Generator;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Examples
{
    public class MaskLayer : ControllerGeneratorBase
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