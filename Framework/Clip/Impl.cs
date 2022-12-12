using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using Debug = System.Diagnostics.Debug;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    internal static class ACCSettingCurveGenericsSupport<T> where T : struct
    {
        public static EditorCurveBinding[] CreateBindings(EditorCurveBinding binding)
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(float) ||
                typeof(T) == typeof(Enum))
                return new[] { binding };
            if (typeof(T) == typeof(Color))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "r"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "g"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "b"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "a"),
                };
            if (typeof(T) == typeof(Vector2) || typeof(T) == typeof(Vector2Int))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "y"),
                };
            if (typeof(T) == typeof(Vector3) || typeof(T) == typeof(Vector3Int))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "z"),
                };
            if (typeof(T) == typeof(Vector4) || typeof(T) == typeof(Quaternion))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "z"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "w"),
                };
            if (typeof(T) == typeof(Rect) || typeof(T) == typeof(RectInt))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "height"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "width"),
                };
            if (typeof(T) == typeof(Bounds))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Center.x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Center.y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Center.z"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Extent.x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Extent.y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Extent.z"),
                };
            if (typeof(T) == typeof(BoundsInt))
                return new[]
                {
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Position.x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Position.y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Position.z"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Size.x"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Size.y"),
                    ACCSettingCurveGenericsSupportUtils.SubBinding(binding, "m_Size.z"),
                };

            ThrowInvalidValueType();
            return Array.Empty<EditorCurveBinding>();
        }

        public static float[] ToFloats(T value)
        {
            if (typeof(T) == typeof(int))
                return new float[] { Unsafe.As<T, int>(ref value) };
            if (typeof(T) == typeof(bool))
                return new[] { Unsafe.As<T, bool>(ref value) ? 1f : 0f };
            if (typeof(T) == typeof(float))
                return new[] { Unsafe.As<T, float>(ref value) };
            if (typeof(T) == typeof(Enum))
                return new float[] { Unsafe.As<T, int>(ref value) };
            if (typeof(T) == typeof(Color))
            {
                var color = Unsafe.As<T, Color>(ref value);
                return new[] { color.r, color.g, color.b, color.a, };
            }

            if (typeof(T) == typeof(Vector2))
            {
                var vector2 = Unsafe.As<T, Vector2>(ref value);
                return new[] { vector2.x, vector2.y };
            }

            if (typeof(T) == typeof(Vector2Int))
            {
                var vector2Int = Unsafe.As<T, Vector2Int>(ref value);
                return new float[] { vector2Int.x, vector2Int.y };
            }

            if (typeof(T) == typeof(Vector3))
            {
                var vector3 = Unsafe.As<T, Vector3>(ref value);
                return new[] { vector3.x, vector3.y, vector3.z };
            }

            if (typeof(T) == typeof(Vector3Int))
            {
                var vector3Int = Unsafe.As<T, Vector3Int>(ref value);
                return new float[] { vector3Int.x, vector3Int.y, vector3Int.z };
            }

            if (typeof(T) == typeof(Vector3Int))
            {
                var vector4 = Unsafe.As<T, Vector4>(ref value);
                return new[] { vector4.x, vector4.y, vector4.z, vector4.w };
            }

            if (typeof(T) == typeof(Quaternion))
            {
                var quaternion = Unsafe.As<T, Quaternion>(ref value);
                return new[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
            }

            if (typeof(T) == typeof(Rect))
            {
                var rect = Unsafe.As<T, Rect>(ref value);
                return new[] { rect.x, rect.y, rect.width, rect.height };
            }

            if (typeof(T) == typeof(RectInt))
            {
                var rectInt = Unsafe.As<T, RectInt>(ref value);
                return new float[] { rectInt.x, rectInt.y, rectInt.width, rectInt.height };
            }

            if (typeof(T) == typeof(Bounds))
            {
                var bounds = Unsafe.As<T, Bounds>(ref value);
                return new[]
                {
                    bounds.center.x, bounds.center.y, bounds.center.z,
                    bounds.extents.x, bounds.extents.y, bounds.extents.z,
                };
            }

            if (typeof(T) == typeof(BoundsInt))
            {
                var boundsInt = Unsafe.As<T, BoundsInt>(ref value);
                return new float[]
                {
                    boundsInt.position.x, boundsInt.position.y, boundsInt.position.z,
                    boundsInt.size.x, boundsInt.size.y, boundsInt.size.z,
                };
            }

            ThrowInvalidValueType();
            return Array.Empty<float>();
        }

        private static void ThrowInvalidValueType()
        {
            throw new ArgumentException(typeof(T).Name + " is not valid animator value type.");
        }
    }

    internal class ACCSettingCurveGenericsSupportUtils
    {
        public static EditorCurveBinding SubBinding(EditorCurveBinding binding, string prop)
        {
            return new EditorCurveBinding
            {
                path = binding.path,
                type = binding.type,
                propertyName = binding.propertyName + "." + prop,
            };
        }
    }

    public sealed class ACCSettingCurve<T> where T : struct
    {
        private readonly AnimationClip _clip;
        private readonly EditorCurveBinding[] _bindings;

        public ACCSettingCurve(AnimationClip clip, EditorCurveBinding binding)
        {
            _clip = clip;
            _bindings = ACCSettingCurveGenericsSupport<T>.CreateBindings(binding);
        }

        public void WithOneFrame(T desiredValue)
        {
            var floats = ACCSettingCurveGenericsSupport<T>.ToFloats(desiredValue);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], ACCClip.OneFrame(floats[i]));
        }

        public void WithFixedSeconds(float seconds, T desiredValue)
        {
            var floats = ACCSettingCurveGenericsSupport<T>.ToFloats(desiredValue);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], ACCClip.ConstantSeconds(seconds, floats[i]));
        }

        public void WithSecondsUnit(Action<ACCSettingKeyframes<T>> action)
        {
            InternalWithUnit(ACCUnit.Seconds, action);
        }

        public void WithFrameCountUnit(Action<ACCSettingKeyframes<T>> action)
        {
            InternalWithUnit(ACCUnit.Frames, action);
        }

        public void WithUnit(ACCUnit unit, Action<ACCSettingKeyframes<T>> action)
        {
            InternalWithUnit(unit, action);
        }

        private void InternalWithUnit(ACCUnit unit, Action<ACCSettingKeyframes<T>> action)
        {
            var mutatedKeyframes = new List<Keyframe>[_bindings.Length];
            for (var i = 0; i < mutatedKeyframes.Length; i++)
                mutatedKeyframes[i] = new List<Keyframe>();
            var builder = new ACCSettingKeyframes<T>(unit, mutatedKeyframes);
            action(builder);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], new AnimationCurve(mutatedKeyframes[i].ToArray()));
        }
    }

    public enum ACCUnit
    {
        Seconds,
        Frames
    }

    public sealed class ACCSettingKeyframes<T> where T : struct
    {
        private readonly ACCUnit _unit;
        private readonly List<Keyframe>[] _mutatedKeyframes;

        public ACCSettingKeyframes(ACCUnit unit, List<Keyframe>[] mutatedKeyframes)
        {
            _unit = unit;
            _mutatedKeyframes = mutatedKeyframes;
        }

        public ACCSettingKeyframes<T> Easing(float timeInUnit, T value)
        {
            var floats = ACCSettingCurveGenericsSupport<T>.ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
                _mutatedKeyframes[i].Add(new Keyframe(AsSeconds(timeInUnit), floats[i], 0, 0));
            return this;
        }

        public ACCSettingKeyframes<T> Constant(float timeInUnit, T value)
        {
            var floats = ACCSettingCurveGenericsSupport<T>.ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
                _mutatedKeyframes[i].Add(new Keyframe(AsSeconds(timeInUnit), floats[i], 0, float.PositiveInfinity));

            return this;
        }

        public ACCSettingKeyframes<T> Linear(float timeInUnit, T value)
        {
            var timeEnd = AsSeconds(timeInUnit);
            var timeStart = _mutatedKeyframes[0].Count == 0 ? timeEnd : _mutatedKeyframes[0].Last().time;

            var floats = ACCSettingCurveGenericsSupport<T>.ToFloats(value);
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

            return this;
        }

        private float AsSeconds(float timeInUnit)
        {
            switch (_unit)
            {
                case ACCUnit.Frames:
                    return timeInUnit / 60f;
                case ACCUnit.Seconds:
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

        public static string CreatePath(UnityEngine.Object obj, LambdaExpression expression)
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
