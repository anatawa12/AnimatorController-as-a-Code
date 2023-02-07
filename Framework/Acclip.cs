using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class AccClip
    {
        internal readonly AnimationClip Clip;
        private readonly AccConfig _config;

        internal AccClip(AnimationClip clip, AccConfig config)
        {
            Clip = clip;
            _config = config;
        }

        public AccClip Toggling(GameObject gameObject, bool value)
        {
            var binding = _config.Binding(gameObject.transform, typeof(GameObject), "m_IsActive");
            AnimationUtility.SetEditorCurve(Clip, binding, OneFrame(value ? 1f : 0f));
            return this;
        }

        internal static AnimationCurve OneFrame(float value) => ConstantSeconds(1 / 60f, value);

        internal static AnimationCurve ConstantSeconds(float seconds, float desiredValue) =>
            AnimationCurve.Constant(0f, seconds, desiredValue);

        public AccClip Animating(Action<AccEditClip> action)
        {
            action.Invoke(new AccEditClip(Clip, _config));
            return this;
        }
    }


    public readonly partial struct AccEditClip
    {
        private readonly AnimationClip _clip;
        private readonly AccConfig _config;

        internal AccEditClip(AnimationClip clip, AccConfig config)
        {
            _clip = clip;
            _config = config;
        }

        public BoolSettingCurve Animates(GameObject gameObject) =>
            AnimatesBool(gameObject.transform, typeof(GameObject), "m_IsActive");

        public EditorCurveBinding BindingFromComponent(Component anyComponent, string propertyName) => 
            _config.Binding(anyComponent.transform, anyComponent.GetType(), propertyName);
    }

    public enum AccUnit
    {
        Seconds,
        Frames
    }
}
