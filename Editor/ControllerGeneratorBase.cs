using System;
using Anatawa12.AnimatorControllerAsACode.Generator;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    public abstract class ControllerGeneratorBase : ScriptableObject
    {
        /// <summary>
        /// The name of Generator. This is used as a prefix of animator layers.
        /// By default, name of class is used.
        /// This name should not contain '_' to avoid conflict.
        /// </summary>
        protected internal virtual string GeneratorName => GetType().Name;

        /// <summary>
        /// Generates Animator Layers.
        /// </summary>
        /// <param name="acaac">Animator As A Code entrypoint object</param>
        protected internal abstract void Generate(ACaaC acaac);

        private void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
