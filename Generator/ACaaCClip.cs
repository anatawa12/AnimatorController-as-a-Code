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

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public class ACaaCClip
    {
        internal readonly AnimationClip Clip;

        public ACaaCClip(AnimationClip clip)
        {
            Clip = clip;
        }

        public ACaaCClip Toggling(GameObject gameObject, bool value)
        {
            var binding = Binding(gameObject.transform, typeof(GameObject), "m_IsActive");
            AnimationUtility.SetEditorCurve(Clip, binding, OneFrame(value ? 1f : 0f));
            return this;
        }

        internal static EditorCurveBinding Binding(Transform transform, Type type, string propertyName) =>
            new EditorCurveBinding
            {
                path = ResolveRelativePath(transform),
                type = type,
                propertyName = propertyName,
            };

        internal static string ResolveRelativePath(Transform transform)
        {
            var elements = new List<string>();
            for(;;)
            {
                elements.Add(transform.name);
                transform = transform.parent;
                if (transform == null) break;
                //TODO: if (transform == root.transform) break;
            }
            elements.Reverse();
            return string.Join("/", elements);
        }

        internal static AnimationCurve OneFrame(float value) => ConstantSeconds(1 / 60f, value);

        internal static AnimationCurve ConstantSeconds(float seconds, float desiredValue) => AnimationCurve.Constant(0f, seconds, desiredValue);

        public ACaaCClip Animating(Action<ACaaCEditClip> action)
        {
            action.Invoke(new ACaaCEditClip(Clip));
            return this;
        }
    }


    public readonly struct ACaaCEditClip
    {
        private readonly AnimationClip _clip;

        public ACaaCEditClip(AnimationClip clip)
        {
            _clip = clip;
        }

        public ACaaCSettingCurve<T> Animates<T>(string path, Type type, string propertyName)
        where T : struct
        {
            var binding = new EditorCurveBinding
            {
                path = path,
                type = type,
                propertyName = propertyName
            };
            return new ACaaCSettingCurve<T>(_clip, binding);
        }

        public ACaaCSettingCurve<T> Animates<T>(Transform transform, Type type, string propertyName)
         where T : struct => 
            Animates<T>(ACaaCClip.ResolveRelativePath(transform), type, propertyName);
        public ACaaCSettingCurve<bool> Animates(GameObject gameObject) => 
            Animates<bool>(gameObject.transform, typeof(GameObject), "m_IsActive");

        public ACaaCSettingCurve<T> Animates<T>(Component component, string property)
         where T : struct =>
            Animates<T>(ACaaCClip.ResolveRelativePath(component.transform), component.GetType(), property);
        
        public ACaaCSettingCurve<T> Animates<TComponent, T>(TComponent component, Expression<Func<TComponent, T>> property)
            where TComponent : Component
            where T : struct=>
            Animates<T>(component, ClipExpressionSupport.CreatePath(component, property));

        public ACaaCSettingCurve<T> AnimatesAnimator<T>(ACaaCParameter<T> floatParameter)
            where T : struct
        {
            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(Animator),
                propertyName = floatParameter.Name
            };
            return new ACaaCSettingCurve<T>(_clip, binding);
        }

        public EditorCurveBinding BindingFromComponent(Component anyComponent, string propertyName)
        {
            return ACaaCClip.Binding(anyComponent.transform, anyComponent.GetType(), propertyName);
        }
    }

    internal static class ACaaCSettingCurveGenericsSupport<T> where T : struct
    {
        public static EditorCurveBinding[] CreateBindings(EditorCurveBinding binding)
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(bool) || typeof(T) == typeof(float) ||
                typeof(T) == typeof(Enum))
                return new[] { binding };
            if (typeof(T) == typeof(Color))
                return new[]
                {
                    SubBinding(binding, "r"),
                    SubBinding(binding, "g"),
                    SubBinding(binding, "b"),
                    SubBinding(binding, "a"),
                };
            if (typeof(T) == typeof(Vector2) || typeof(T) == typeof(Vector2Int))
                return new[]
                {
                    SubBinding(binding, "x"),
                    SubBinding(binding, "y"),
                };
            if (typeof(T) == typeof(Vector3) || typeof(T) == typeof(Vector3Int))
                return new[]
                {
                    SubBinding(binding, "x"),
                    SubBinding(binding, "y"),
                    SubBinding(binding, "z"),
                };
            if (typeof(T) == typeof(Vector4) || typeof(T) == typeof(Quaternion))
                return new[]
                {
                    SubBinding(binding, "x"),
                    SubBinding(binding, "y"),
                    SubBinding(binding, "z"),
                    SubBinding(binding, "w"),
                };
            if (typeof(T) == typeof(Rect) || typeof(T) == typeof(RectInt))
                return new[]
                {
                    SubBinding(binding, "x"),
                    SubBinding(binding, "y"),
                    SubBinding(binding, "height"),
                    SubBinding(binding, "width"),
                };
            if (typeof(T) == typeof(Bounds))
                return new[]
                {
                    SubBinding(binding, "m_Center.x"),
                    SubBinding(binding, "m_Center.y"),
                    SubBinding(binding, "m_Center.z"),
                    SubBinding(binding, "m_Extent.x"),
                    SubBinding(binding, "m_Extent.y"),
                    SubBinding(binding, "m_Extent.z"),
                };
            if (typeof(T) == typeof(BoundsInt))
                return new[]
                {
                    SubBinding(binding, "m_Position.x"),
                    SubBinding(binding, "m_Position.y"),
                    SubBinding(binding, "m_Position.z"),
                    SubBinding(binding, "m_Size.x"),
                    SubBinding(binding, "m_Size.y"),
                    SubBinding(binding, "m_Size.z"),
                };

            throw new ArgumentException(typeof(T).Name + " is not valid animator value type.");
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

            return ThrowInvalidValueType<float[]>();
        }

        private static TReturns ThrowInvalidValueType<TReturns>()
        {
            throw new ArgumentException(typeof(T).Name + " is not valid animator value type.");
        }

        private static EditorCurveBinding SubBinding(EditorCurveBinding binding, string prop)
        {
            return new EditorCurveBinding
            {
                path = binding.path,
                type = binding.type,
                propertyName = binding.propertyName + "." + prop,
            };
        }
    }

    public sealed class ACaaCSettingCurve<T> where T : struct
    {
        private readonly AnimationClip _clip;
        private readonly EditorCurveBinding[] _bindings;

        public ACaaCSettingCurve(AnimationClip clip, EditorCurveBinding binding)
        {
            _clip = clip;
            _bindings = ACaaCSettingCurveGenericsSupport<T>.CreateBindings(binding);
        }

        public void WithOneFrame(T desiredValue)
        {
            var floats = ACaaCSettingCurveGenericsSupport<T>.ToFloats(desiredValue);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], ACaaCClip.OneFrame(floats[i]));
        }

        public void WithFixedSeconds(float seconds, T desiredValue)
        {
            var floats = ACaaCSettingCurveGenericsSupport<T>.ToFloats(desiredValue);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], ACaaCClip.ConstantSeconds(seconds, floats[i]));
        }

        public void WithSecondsUnit(Action<ACaaCSettingKeyframes<T>> action)
        {
            InternalWithUnit(ACaaCUnit.Seconds, action);
        }

        public void WithFrameCountUnit(Action<ACaaCSettingKeyframes<T>> action)
        {
            InternalWithUnit(ACaaCUnit.Frames, action);
        }

        public void WithUnit(ACaaCUnit unit, Action<ACaaCSettingKeyframes<T>> action)
        {
            InternalWithUnit(unit, action);
        }

        private void InternalWithUnit(ACaaCUnit unit, Action<ACaaCSettingKeyframes<T>> action)
        {
            var mutatedKeyframes = new List<Keyframe>[_bindings.Length];
            for (var i = 0; i < mutatedKeyframes.Length; i++)
                mutatedKeyframes[i] = new List<Keyframe>();
            var builder = new ACaaCSettingKeyframes<T>(unit, mutatedKeyframes);
            action(builder);
            for (var i = 0; i < _bindings.Length; i++)
                AnimationUtility.SetEditorCurve(_clip, _bindings[i], new AnimationCurve(mutatedKeyframes[i].ToArray()));
        }
    }

    public enum ACaaCUnit
    {
        Seconds, Frames
    }

    public sealed class ACaaCSettingKeyframes<T> where T : struct
    {
        private readonly ACaaCUnit _unit;
        private readonly List<Keyframe>[] _mutatedKeyframes;

        public ACaaCSettingKeyframes(ACaaCUnit unit, List<Keyframe>[] mutatedKeyframes)
        {
            _unit = unit;
            _mutatedKeyframes = mutatedKeyframes;
        }

        public ACaaCSettingKeyframes<T> Easing(float timeInUnit, T value)
        {
            var floats = ACaaCSettingCurveGenericsSupport<T>.ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
                _mutatedKeyframes[i].Add(new Keyframe(AsSeconds(timeInUnit), floats[i], 0, 0));
            return this;
        }

        public ACaaCSettingKeyframes<T> Constant(float timeInUnit, T value)
        {
            var floats = ACaaCSettingCurveGenericsSupport<T>.ToFloats(value);
            for (var i = 0; i < _mutatedKeyframes.Length; i++)
                _mutatedKeyframes[i].Add(new Keyframe(AsSeconds(timeInUnit), floats[i], 0, float.PositiveInfinity));

            return this;
        }

        public ACaaCSettingKeyframes<T> Linear(float timeInUnit, T value)
        {
            var timeEnd = AsSeconds(timeInUnit);
            var timeStart = _mutatedKeyframes[0].Count == 0 ? timeEnd : _mutatedKeyframes[0].Last().time;

            var floats = ACaaCSettingCurveGenericsSupport<T>.ToFloats(value);
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
                case ACaaCUnit.Frames:
                    return timeInUnit / 60f;
                case ACaaCUnit.Seconds:
                    return timeInUnit;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
