using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class AccParameter<T>
        where T : unmanaged
    {
        private readonly AnimatorControllerParameter _parameter;

        public AccParameter(AnimatorControllerParameter parameter)
        {
            Utils.CheckAnimationParameterType<T>();
            _parameter = parameter;
        }

        public string Name => _parameter.name;

        public AccParameterCondition IsEqualTo(T value)
        {
            if (typeof(T) == typeof(bool))
                return BoolCondition(Unsafe.As<T, bool>(ref value));
            return new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.Equals, ToFloat(value), Name));
        }

        public AccParameterCondition IsNotEqualTo(T value)
        {
            if (typeof(T) == typeof(bool))
                return BoolCondition(!Unsafe.As<T, bool>(ref value));
            return new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.NotEqual, ToFloat(value), Name));
        }

        private AccParameterCondition BoolCondition(bool flg) =>
            new AccParameterCondition(
                new AccParameterSingleCondition(flg ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, Name));

        public AccParameter<T> DefaultValue(T value)
        {
            if (typeof(T) == typeof(float))
                _parameter.defaultFloat = Unsafe.As<T, float>(ref value);
            else if (typeof(T) == typeof(int))
                _parameter.defaultInt = Unsafe.As<T, int>(ref value);
            else if (typeof(T) == typeof(bool))
                _parameter.defaultBool = Unsafe.As<T, bool>(ref value);
            else if (typeof(T).IsEnum && Enum.GetUnderlyingType(typeof(T)) == typeof(int))
                _parameter.defaultInt = Unsafe.As<T, int>(ref value);
            return this;
        }

        public float ToFloat(T value) => Utils.AnimationParameterToFloat(value);
    }

    public static class AccTypeSpecificMethods
    {
        public static AccParameterCondition IsFalse(this AccParameter<bool> self) => self.IsEqualTo(false);
        public static AccParameterCondition IsTrue(this AccParameter<bool> self) => self.IsEqualTo(true);

        public static AccParameterCondition IsLessThan(this AccParameter<int> self, int threshold) => 
            new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.Less, threshold, self.Name));
        public static AccParameterCondition IsGraterThan(this AccParameter<int> self, int threshold) => 
            new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.Greater, threshold, self.Name));
        
        public static AccParameterCondition IsLessThan(this AccParameter<float> self, float threshold) => 
            new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.Less, threshold, self.Name));
        public static AccParameterCondition IsGraterThan(this AccParameter<float> self, float threshold) => 
            new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.Greater, threshold, self.Name));
    }

    public readonly struct AccParameterCondition
    {
        private readonly AccParameterSingleCondition[] _conditions;

        internal AccParameterCondition(AccParameterSingleCondition condition)
        {
            _conditions = new[]
            {
                condition
            };
        }

        private AccParameterCondition(AccParameterSingleCondition[] conditions)
        {
            _conditions = conditions;
        }

        public AccParameterCondition And(AccParameterCondition other)
        {
            if (other._conditions == null || other._conditions.Length == 0)
                return this;
            if (_conditions == null || _conditions.Length == 0)
                return other;
            var result = new AccParameterSingleCondition[_conditions.Length + other._conditions.Length];
            Array.Copy(_conditions, result, _conditions.Length);
            Array.Copy(other._conditions, 0, result, _conditions.Length, other._conditions.Length);
            return new AccParameterCondition(result);
        }

        public void ApplyTo(AnimatorTransitionBase transition)
        {
            if (_conditions == null) return;

            foreach (var condition in _conditions)
            {
                transition.AddCondition(condition.Mode, condition.Threshold, condition.Parameter);
            }
        }
    }

    internal readonly struct AccParameterSingleCondition
    {
        internal readonly AnimatorConditionMode Mode;
        internal readonly float Threshold;
        internal readonly string Parameter;

        public AccParameterSingleCondition(AnimatorConditionMode mode, float threshold, string parameter)
        {
            Mode = mode;
            Threshold = threshold;
            Parameter = parameter;
        }
    }
}
