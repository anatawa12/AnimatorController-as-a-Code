using System;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    /// <summary>
    /// The attribute to Reference Objects in a prefab.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InPrefabReferenceAttribute : PropertyAttribute { }
}
