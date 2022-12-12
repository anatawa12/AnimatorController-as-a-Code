
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Anatawa12.AnimatorControllerAsACode.Framework.Clip;
using UnityEditor;
using UnityEngine;

// we cannot generate for Enum so I write hand here
namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class EnumSettingCurve<T> where T : Enum
    {
        private readonly SettingCurveImpl<T, EnumTypeInfo<T>, Fixed1<EditorCurveBinding>, Fixed1<float>,
            Fixed1<List<Keyframe>>> _impl;

        internal EnumSettingCurve(AnimationClip clip, EditorCurveBinding binding)
        {
            _impl =
                new SettingCurveImpl<T, EnumTypeInfo<T>, Fixed1<EditorCurveBinding>, Fixed1<float>,
                    Fixed1<List<Keyframe>>>(clip, binding);
        }

        public EnumSettingCurve<T> WithOneFrame(T desiredValue)
        {
            _impl.WithOneFrame(desiredValue);
            return this;
        }

        public EnumSettingCurve<T> WithFixedSeconds(float seconds, T desiredValue)
        {
            _impl.WithFixedSeconds(seconds, desiredValue);
            return this;
        }

        public EnumSettingCurve<T> WithSecondsUnit(Action<EnumSettingKeyframes<T>> action)
        {
            _impl.WithSecondsUnit(impl => action(new EnumSettingKeyframes<T>(impl)));
            return this;
        }

        public EnumSettingCurve<T> WithFrameCountUnit(Action<EnumSettingKeyframes<T>> action)
        {
            _impl.WithFrameCountUnit(impl => action(new EnumSettingKeyframes<T>(impl)));
            return this;
        }

        public EnumSettingCurve<T> WithUnit(ACCUnit unit, Action<EnumSettingKeyframes<T>> action)
        {
            _impl.WithUnit(unit, impl => action(new EnumSettingKeyframes<T>(impl)));
            return this;
        }

    }

    public class EnumSettingKeyframes<T> where T : Enum
    {
        private readonly SettingKeyframesImpl<T, EnumTypeInfo<T>, Fixed1<EditorCurveBinding>, Fixed1<float>,
            Fixed1<List<Keyframe>>> _impl;

        internal EnumSettingKeyframes(
            SettingKeyframesImpl<T, EnumTypeInfo<T>, Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>>
                impl)
        {
            _impl = impl;
        }

        public EnumSettingKeyframes<T> Easing(float timeInUnit, T value)
        {
            _impl.Easing(timeInUnit, value);
            return this;
        }

        public EnumSettingKeyframes<T> Constant(float timeInUnit, T value)
        {
            _impl.Constant(timeInUnit, value);
            return this;
        }

        public EnumSettingKeyframes<T> Linear(float timeInUnit, T value)
        {
            _impl.Linear(timeInUnit, value);
            return this;
        }
    }

    public partial struct ACCEditClip
    {
        public EnumSettingCurve<T> AnimatesEnum<T>(string path, Type type, string propertyName)
            where T : Enum
        {
            if (typeof(T).UnderlyingSystemType != typeof(int))
                throw new ArgumentException("T must be a enum with UnderlyingSystemType int", nameof(T));
            var binding = new EditorCurveBinding
            {
                path = path,
                type = type,
                propertyName = propertyName
            };
            return new EnumSettingCurve<T>(_clip, binding);
        }

        public EnumSettingCurve<T> AnimatesEnum<T>(Transform transform, Type type, string propertyName)
            where T : Enum => AnimatesEnum<T>(ACCClip.ResolveRelativePath(transform), type, propertyName);

        public EnumSettingCurve<T> AnimatesEnum<T>(Component component, string property)
            where T : Enum => AnimatesEnum<T>(ACCClip.ResolveRelativePath(component.transform), component.GetType(),
            property);

        public EnumSettingCurve<T> Animates<TComponent, T>(TComponent component, Expression<Func<TComponent, int>> property)
            where TComponent : Component
            where T : Enum => AnimatesEnum<T>(component, ClipExpressionSupport.CreatePath(component, property));
    }
}
