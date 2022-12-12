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
        /// This list should not be modified unless user input is changed.
        /// </summary>
        // TODO: better limitation on modification. something like SetDirty can be used.
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
