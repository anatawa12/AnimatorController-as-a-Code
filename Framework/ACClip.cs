using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class ACCClip
    {
        internal readonly AnimationClip Clip;

        public ACCClip(AnimationClip clip)
        {
            Clip = clip;
        }

        public ACCClip Toggling(GameObject gameObject, bool value)
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

        public ACCClip Animating(Action<ACCEditClip> action)
        {
            action.Invoke(new ACCEditClip(Clip));
            return this;
        }
    }


    public readonly struct ACCEditClip
    {
        private readonly AnimationClip _clip;

        public ACCEditClip(AnimationClip clip)
        {
            _clip = clip;
        }

        public ACCSettingCurve<T> Animates<T>(string path, Type type, string propertyName)
            where T : struct
        {
            var binding = new EditorCurveBinding
            {
                path = path,
                type = type,
                propertyName = propertyName
            };
            return new ACCSettingCurve<T>(_clip, binding);
        }

        public ACCSettingCurve<T> Animates<T>(Transform transform, Type type, string propertyName)
            where T : struct =>
            Animates<T>(ACCClip.ResolveRelativePath(transform), type, propertyName);

        public ACCSettingCurve<bool> Animates(GameObject gameObject) =>
            Animates<bool>(gameObject.transform, typeof(GameObject), "m_IsActive");

        public ACCSettingCurve<T> Animates<T>(Component component, string property)
            where T : struct =>
            Animates<T>(ACCClip.ResolveRelativePath(component.transform), component.GetType(), property);

        public ACCSettingCurve<T> Animates<TComponent, T>(TComponent component,
            Expression<Func<TComponent, T>> property)
            where TComponent : Component
            where T : struct =>
            Animates<T>(component, ClipExpressionSupport.CreatePath(component, property));

        public ACCSettingCurve<T> AnimatesAnimator<T>(ACCParameter<T> floatParameter)
            where T : struct
        {
            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(Animator),
                propertyName = floatParameter.Name
            };
            return new ACCSettingCurve<T>(_clip, binding);
        }

        public EditorCurveBinding BindingFromComponent(Component anyComponent, string propertyName)
        {
            return ACCClip.Binding(anyComponent.transform, anyComponent.GetType(), propertyName);
        }
    }
}
