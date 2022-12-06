using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;

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

        private static readonly Dictionary<PropertyInfo, String> Mapping = MakeMapping();

        private static Dictionary<PropertyInfo, String> MakeMapping()
        {
            var map = new Dictionary<PropertyInfo, string>();

            void AddMPrefixed<T>(params Expression<Func<T, object>>[] functions)
                where T: UnityEngine.Object
            {
                foreach (var func in functions)
                {
                    var prop = PropertyInfo(func);
                    map[prop] = "m_" + char.ToUpper(prop.Name[0], CultureInfo.InvariantCulture) +
                                prop.Name.Substring(1);
                }
            }

            // TODO: add more types

            AddMPrefixed<Transform>(
                x => x.localPosition,
                x => x.localRotation,
                x => x.localScale);

            AddMPrefixed<RectTransform>(
                x => x.anchorMax,
                x => x.anchorMin,
                x => x.pivot,
                x => x.sizeDelta);

            map[PropertyInfo<PositionConstraint>(x => x.constraintActive)] = "m_Active";

            map[PropertyInfo<GameObject>(x => x.activeSelf)] = "m_IsActive";

            return map;
        }

        public static string CreatePath<TC, T>(Expression<Func<TC, T>> expression)
            where TC : UnityEngine.Component
        {
            var param = expression.Parameters[0];
            return CreatePath(expression.Body, param);
        }

        private static string CreatePath(Expression expression, ParameterExpression param)
        {
            string result = null;
            var root = true;
            while (true)
            {
                if (expression == param)
                {
                    return result;
                }

                if (!root && expression.Type.IsSubclassOf(typeof(UnityEngine.Object)))
                    throw new InvalidOperationException("Cannot change fields of UnityEngine.Object except root.");

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var access = (MemberExpression)expression;
                    string memberName = CheckAndComputeMemberName(access.Member, root);

                    expression = access.Expression;
                    result = result == null ? memberName : memberName + "." + result;
                    root = false;
                    continue;
                }

                throw new InvalidOperationException($"unsupported kind of expression: {expression}");
            }
        }

        private static string CheckAndComputeMemberName(MemberInfo member, bool root)
        {
            if (member is FieldInfo field)
            {
                if (!IsSerializeField(field))
                    throw new InvalidOperationException($"Cannot change non-serializable field {field}");
                return field.Name;
            }

            var prop = (PropertyInfo)member;

            if (root && !prop.CanWrite)
                throw new InvalidOperationException("Cannot change get-only property");

            // 1st path: try mapped builtin types
            if (Mapping.TryGetValue(prop, out var memberName))
                return memberName;

            // 2nd: try backing field with same name
            System.Diagnostics.Debug.Assert(prop.ReflectedType != null, "prop.ReflectedType != null");
            var backingField = prop.ReflectedType.GetField(prop.Name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (IsSerializeField(backingField) && backingField != null)
                return backingField.Name;

            // 3rd: try backing field with 'm_' prefix
            var mFieldName = "m_" + char.ToUpper(prop.Name[0], CultureInfo.InvariantCulture) + prop.Name.Substring(1);
            var mBackingField = prop.ReflectedType.GetField(mFieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (IsSerializeField(mBackingField) && mBackingField != null)
                return mBackingField.Name;

            throw new InvalidOperationException($"Backing field for {prop} not found.");
        }

        // TODO: field type check
        private static bool IsSerializeField(FieldInfo backingField) =>
            (backingField.IsPublic || backingField.GetCustomAttribute<SerializeField>() != null)
            && !backingField.IsStatic
            && !backingField.IsInitOnly;
    }
}
