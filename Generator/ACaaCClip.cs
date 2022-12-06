using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using Debug = System.Diagnostics.Debug;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public class ACaaCClip
    {
        private readonly AnimationClip _clip;

        public ACaaCClip(AnimationClip clip)
        {
            _clip = clip;
        }
    }

    internal static class ClipExpressionSupport
    {
        private static PropertyInfo PropertyInfo<T>(Expression<Func<T, object>> func)
            where T: UnityEngine.Object
        {
            var expr = (func.Body as UnaryExpression)?.Operand ?? func.Body;
            var memberExpression = (MemberExpression)expr;
            return (PropertyInfo)memberExpression.Member;
        }

        // extra names mapping
        private static readonly Dictionary<MemberInfo, String> Mapping = new Dictionary<MemberInfo, string>
        {
            [PropertyInfo<PositionConstraint>(x => x.constraintActive)] = "m_Active",
            [PropertyInfo<GameObject>(x => x.activeSelf)] = "m_IsActive",
        };

        public static string CreatePath<TC, T>(TC obj, Expression<Func<TC, T>> expression)
            where TC : UnityEngine.Object
        {
            var param = expression.Parameters[0];
            var memberInfoPath = CollectMemberInfoPath(expression.Body, param);
            using (var serialized = new SerializedObject(obj))
            {
                return CreatePath(serialized, memberInfoPath);
            }
        }

        private static List<MemberInfo> CollectMemberInfoPath(Expression expression, ParameterExpression param)
        {
            List<MemberInfo> result = new List<MemberInfo>();
            while (true)
            {
                if (expression == param)
                {
                    if (result.Count == 0)
                        throw new InvalidOperationException("Cannot change object itself.");
                    result.Reverse();
                    return result;
                }

                // We cannot change properties of UnityEngine.Object fields.
                // We must change UnityEngine.Object's properties directly.
                if (result.Count != 0 && expression.Type.IsSubclassOf(typeof(UnityEngine.Object)))
                    throw new InvalidOperationException("Cannot change fields of UnityEngine.Object except root.");

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var access = (MemberExpression)expression;
                    expression = access.Expression;
                    result.Add(access.Member);
                    continue;
                }

                throw new InvalidOperationException($"unsupported kind of expression: {expression}");
            }
        }
        
        private static string CreatePath(SerializedObject obj, List<MemberInfo> path)
        {
            string building = null;
            SerializedProperty prop = null;
            if (path.Count == 0)
                throw new ArgumentException("must not empty", nameof(path));
            foreach (var info in path)
            {
                building = FindPropertyPath(obj, building, info)
                           ?? throw new InvalidOperationException($"Serialized Property not found");
                prop = obj.FindProperty(building)
                       ?? throw new InvalidOperationException(
                           "BUG: FindPropertyName returns property name not exists.");
            }

            Debug.Assert(prop != null, nameof(prop) + " != null");
            Debug.Assert(building != null, nameof(building) + " != null");
            if (!IsAnimatablePropertyType(prop.propertyType))
                throw new InvalidOperationException("BUG: Specified Animation Property is not animatable");

            return building;
        }

        private static bool IsAnimatablePropertyType(SerializedPropertyType propertyType)
        {
            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Enum:
                    return true;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.ManagedReference:
                default: 
                    return false;
            }
        }

        private static string FindPropertyPath(SerializedObject obj, string building, MemberInfo info)
        {
            // first, find in mapping
            if (Mapping.TryGetValue(info, out var value))
                return ConcatPath(building, value);

            // then find property with name
            return obj.FindProperty(info.Name)?.name
                       ?? obj.FindProperty(MPrefixedName(info.Name))?.name;
        }

        private static string ConcatPath(string a, string b) => a == null ? b : b == null ? a : a + '.' + b;

        private static string MPrefixedName(string name)
        {
            return "m_" + char.ToUpperInvariant(name[0]) + name.Substring(1);
        }
    }
}
