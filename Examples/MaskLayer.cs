using Anatawa12.AnimatorAsACode.Editor;
using Anatawa12.AnimatorAsACode.Generator;
using UnityEngine;

namespace Anatawa12.AnimatorAsACode.Examples
{
    public class MaskLayer : ControllerGeneratorBase
    {
        public AvatarMask mask;

        // optional: GetType().Name by default
        protected override string GeneratorName => "MaskLayer";

        protected override void Generate(AaaC aaac)
        {
            aaac.AddMainLayer().WithMask(mask);
        }
    }
}