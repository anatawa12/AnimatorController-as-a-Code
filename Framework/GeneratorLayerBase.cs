using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class GeneratorLayerBase : ScriptableObject
    {
        /// <summary>
        /// The name of Generator. This is used as a prefix of animator layers.
        /// By default, name of class is used.
        /// This name should not contain '_' to avoid conflict.
        /// </summary>
        protected internal virtual string GeneratorName => GetType().Name;

        /// <summary>
        /// The list of watching objects.
        /// </summary>
        /// <remarks>
        /// Don't forget to add some assets to this list.
        /// If you forget, auto regeneration do not work as expected.
        /// If you added too many assets to this list, It's OK.
        /// it will make more regeneration than required but nothing will be broken.
        ///
        /// If your implementation <b>copies</b> or <b>uses</b> some part of asset to AnimatorController,
        /// You <b>must</b> include the asset to this list. If you forget, automatic generation will not work as expect
        ///
        /// If your implementation just make generated AnimatorController <b>reference</b> some asset,
        /// You <b>don't have to </b> include the asset to this list.
        /// </remarks>
        /// <example>
        /// <c>CopyFromOtherControllerLayer</c> (in example module) copies some part of <c>controller.layers</c>
        /// so it adds <c>controller</c> to <c>WatchingObjects</c>.
        /// </example>
        /// <example>
        /// <c>FacialExpression</c> (in example module) do not copies nor uses part of <c>Motion</c>
        /// so It doesn't add motions to <c>WatchingObjects</c>.
        /// </example>
        protected internal virtual IEnumerable<Object> WatchingObjects => Array.Empty<Object>();

        /// <summary>
        /// Generates Animator Layers.
        /// </summary>
        /// <param name="acc">Animator As A Code entrypoint object</param>
        protected internal abstract void Generate(Acc acc);

        private void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
