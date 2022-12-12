using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using Debug = System.Diagnostics.Debug;

namespace Anatawa12.AnimatorControllerAsACode.Framework.Clip
{
    internal readonly struct SettingCurveImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray> 
        where TInfo: struct, ISettingCurveTypeInfo<T, TBindingArray, TFloatArray, TKeyframeListArray>
        where TBindingArray: struct, IFixedArray<EditorCurveBinding>
        where TFloatArray: struct, IFixedArray<float>
        where TKeyframeListArray: struct, IFixedArray<List<Keyframe>>
    {
        private readonly AnimationClip _clip;
        private readonly TBindingArray _bindings;

        internal SettingCurveImpl(AnimationClip clip, EditorCurveBinding binding)
        {
            _clip = clip;
            _bindings = default(TInfo).CreateBindings(binding);
        }

        internal void WithOneFrame(T desiredValue)
        {
            var floats = default(TInfo).ToFloats(desiredValue);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], AccClip.OneFrame(floats[i]));
        }

        internal void WithFixedSeconds(float seconds, T desiredValue)
        {
            var floats = default(TInfo).ToFloats(desiredValue);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], AccClip.ConstantSeconds(seconds, floats[i]));
        }

        internal void WithSecondsUnit(Action<SettingKeyframesImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray>> action)
        {
            InternalWithUnit(AccUnit.Seconds, action);
        }

        internal void WithFrameCountUnit(Action<SettingKeyframesImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray>> action)
        {
            InternalWithUnit(AccUnit.Frames, action);
        }

        internal void WithUnit(AccUnit unit, Action<SettingKeyframesImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray>> action)
        {
            InternalWithUnit(unit, action);
        }

        private void InternalWithUnit(AccUnit unit, Action<SettingKeyframesImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray>> action)
        {
            var mutatedKeyframes = default(TInfo).CreateKeyframeLists();
            var builder = new SettingKeyframesImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray>(unit, mutatedKeyframes);
            action(builder);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], new AnimationCurve(mutatedKeyframes[i].ToArray()));
        }
    }

    internal readonly struct SettingKeyframesImpl<T, TInfo, TBindingArray, TFloatArray, TKeyframeListArray>
        where TInfo: struct, ISettingCurveTypeInfo<T, TBindingArray, TFloatArray, TKeyframeListArray>
        where TBindingArray: struct, IFixedArray<EditorCurveBinding>
        where TFloatArray: struct, IFixedArray<float>
        where TKeyframeListArray: struct, IFixedArray<List<Keyframe>>
    {
        private readonly AccUnit _unit;
        private readonly TKeyframeListArray _mutatedKeyframes;

        internal SettingKeyframesImpl(AccUnit unit, TKeyframeListArray mutatedKeyframes)
        {
            _unit = unit;
            _mutatedKeyframes = mutatedKeyframes;
        }

        internal void Easing(float timeInUnit, T value)
        {
            var floats = default(TInfo).ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
                _mutatedKeyframes[i].Add(new Keyframe(AsSeconds(timeInUnit), floats[i], 0, 0));
        }

        internal void Constant(float timeInUnit, T value)
        {
            var floats = default(TInfo).ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
                _mutatedKeyframes[i].Add(new Keyframe(AsSeconds(timeInUnit), floats[i], 0, float.PositiveInfinity));

        }

        internal void Linear(float timeInUnit, T value)
        {
            var timeEnd = AsSeconds(timeInUnit);
            var timeStart = _mutatedKeyframes[0].Count == 0 ? timeEnd : _mutatedKeyframes[0].Last().time;

            var floats = default(TInfo).ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
            {
                var mutatedKeyframes = _mutatedKeyframes[i];

                var valueEnd = floats[i];
                var valueStart = mutatedKeyframes.Count == 0 ? valueEnd : mutatedKeyframes.Last().value;
                var num = (float)(((double)valueEnd - (double)valueStart) / ((double)timeEnd - (double)timeStart));
                // FIXME: This can cause NaN tangents which messes everything

                // return new AnimationCurve(new Keyframe[2]
                // {
                // new Keyframe(timeStart, valueStart, 0.0f, num),
                // new Keyframe(timeEnd, valueEnd, num, 0.0f)
                // });

                if (mutatedKeyframes.Count > 0)
                {
                    var lastKeyframe = mutatedKeyframes.Last();
                    lastKeyframe.outTangent = num;
                    mutatedKeyframes[mutatedKeyframes.Count - 1] = lastKeyframe;
                }

                mutatedKeyframes.Add(new Keyframe(AsSeconds(timeInUnit), valueEnd, num, 0.0f));
            }
        }

        private float AsSeconds(float timeInUnit)
        {
            switch (_unit)
            {
                case AccUnit.Frames:
                    return timeInUnit / 60f;
                case AccUnit.Seconds:
                    return timeInUnit;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal static class ClipExpressionSupport
    {
        private static PropertyInfo PropertyInfo<T>(Expression<Func<T, object>> func)
            where T : UnityEngine.Object
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

        internal static string CreatePath(UnityEngine.Object obj, LambdaExpression expression)
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
                case SerializedPropertyType.Color:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                    return true;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.String:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
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
