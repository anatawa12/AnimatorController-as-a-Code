using System;
using System.Collections.Generic;
using Anatawa12.AnimatorControllerAsACode.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Examples
{
    public class MaskLayer : GeneratorLayerBase
    {
        public AvatarMask mask;

        // optional: GetType().Name by default
        protected override string DefaultName => "MaskLayer";
        protected override IEnumerable<Object> WatchingObjects => Array.Empty<Object>();

        protected override void Generate(Acc acc)
        {
            acc.AddMainLayer().WithMask(mask);
        }
    }
}
