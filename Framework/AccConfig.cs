using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class AccConfig
    {
        public Vector2 StateOffset = new Vector2(250, 70);
        public Vector2 FirstStateAt = new Vector2(1, 1);
        public Transform RootTransform;

        public AccConfig(Transform rootTransform)
        {
            RootTransform = rootTransform;
        }

        internal EditorCurveBinding Binding(Transform transform, Type type, string propertyName) =>
            new EditorCurveBinding
            {
                path = ResolveRelativePath(transform),
                type = type,
                propertyName = propertyName,
            };

        internal string ResolveRelativePath(Transform transform)
        {
            var elements = new List<string>();
            for (; transform != null && transform != RootTransform; transform = transform.parent)
            {
                elements.Add(transform.name);
            }

            elements.Reverse();
            return string.Join("/", elements);
        }
    }
}
