using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class AccParameter<T>
    {
        private readonly AnimatorControllerParameter _parameter;
        private readonly Func<T, float> _toFloat;

        public AccParameter(AnimatorControllerParameter parameter, Func<T, float> toFloat)
        {
            _parameter = parameter;
            _toFloat = toFloat;
        }

        public string Name => _parameter.name;

        public AccParameterCondition IsEqualTo(T value)
        {
            return new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.Equals, _toFloat(value), Name));
        }

        public AccParameterCondition IsNotEqualTo(T value)
        {
            return new AccParameterCondition(
                new AccParameterSingleCondition(AnimatorConditionMode.NotEqual, _toFloat(value), Name));
        }
    }

    public static class AccTypeSpecificMethods
    {
        public static AccParameterCondition IsFalse(this AccParameter<bool> self) => self.IsEqualTo(false);
        public static AccParameterCondition IsTrue(this AccParameter<bool> self) => self.IsEqualTo(true);
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

        public void ApplyTo(AnimatorStateTransition transition)
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
