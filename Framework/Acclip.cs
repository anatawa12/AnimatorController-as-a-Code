using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class AccClip
    {
        internal readonly AnimationClip Clip;

        public AccClip(AnimationClip clip)
        {
            Clip = clip;
        }

        public AccClip Toggling(GameObject gameObject, bool value)
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
            for (;;)
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

        internal static AnimationCurve ConstantSeconds(float seconds, float desiredValue) =>
            AnimationCurve.Constant(0f, seconds, desiredValue);

        public AccClip Animating(Action<AccEditClip> action)
        {
            action.Invoke(new AccEditClip(Clip));
            return this;
        }
    }


    public readonly partial struct AccEditClip
    {
        private readonly AnimationClip _clip;

        public AccEditClip(AnimationClip clip)
        {
            _clip = clip;
        }

        public BoolSettingCurve Animates(GameObject gameObject) =>
            AnimatesBool(gameObject.transform, typeof(GameObject), "m_IsActive");

        private EditorCurveBinding AnimatorBinding<T>(AccParameter<T> floatParameter) => new EditorCurveBinding
        {
            path = "",
            type = typeof(Animator),
            propertyName = floatParameter.Name
        };

        public BoolSettingCurve AnimatesAnimator(AccParameter<bool> parameter) => new BoolSettingCurve(_clip, AnimatorBinding(parameter));
        public FloatSettingCurve AnimatesAnimator(AccParameter<float> parameter) => new FloatSettingCurve(_clip, AnimatorBinding(parameter));
        public IntSettingCurve AnimatesAnimator(AccParameter<int> parameter) => new IntSettingCurve(_clip, AnimatorBinding(parameter));
        public EnumSettingCurve<T> AnimatesAnimator<T>(AccParameter<T> parameter)
            where T : Enum
            => new EnumSettingCurve<T>(_clip, AnimatorBinding(parameter));

        public EditorCurveBinding BindingFromComponent(Component anyComponent, string propertyName) => 
            AccClip.Binding(anyComponent.transform, anyComponent.GetType(), propertyName);
    }

    public enum AccUnit
    {
        Seconds,
        Frames
    }
}
