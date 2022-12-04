using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    public class ACaaCParameter<T>
    {
        private readonly AnimatorControllerParameter _parameter;
        private readonly Func<T, float> _toFloat;

        public ACaaCParameter(AnimatorControllerParameter parameter, Func<T, float> toFloat)
        {
            _parameter = parameter;
            _toFloat = toFloat;
        }

        public string Name => _parameter.name;

        public ACaaCParameterCondition IsEqualTo(T value)
        {
            return new ACaaCParameterCondition(
                new ACaaCParameterSingleCondition(AnimatorConditionMode.Equals, _toFloat(value), Name));
        }

        public ACaaCParameterCondition IsNotEqualTo(T value)
        {
            return new ACaaCParameterCondition(
                new ACaaCParameterSingleCondition(AnimatorConditionMode.NotEqual, _toFloat(value), Name));
        }
    }

    public static class ACaaCTypeSpecificMethods
    {
        public static ACaaCParameterCondition IsFalse(this ACaaCParameter<bool> self) => self.IsEqualTo(false);
        public static ACaaCParameterCondition IsTrue(this ACaaCParameter<bool> self) => self.IsEqualTo(true);
    }

    public readonly struct ACaaCParameterCondition
    {
        private readonly ACaaCParameterSingleCondition[] _conditions;

        internal ACaaCParameterCondition(ACaaCParameterSingleCondition condition)
        {
            _conditions = new[]
            {
                condition
            };
        }

        private ACaaCParameterCondition(ACaaCParameterSingleCondition[] conditions)
        {
            _conditions = conditions;
        }

        public ACaaCParameterCondition And(ACaaCParameterCondition other)
        {
            if (other._conditions == null || other._conditions.Length == 0)
                return this;
            if (_conditions == null || _conditions.Length == 0)
                return other;
            var result = new ACaaCParameterSingleCondition[_conditions.Length + other._conditions.Length];
            Array.Copy(_conditions, result, _conditions.Length);
            Array.Copy(other._conditions, 0, result, _conditions.Length, other._conditions.Length);
            return new ACaaCParameterCondition(result);
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

    internal readonly struct ACaaCParameterSingleCondition
    {
        internal readonly AnimatorConditionMode Mode;
        internal readonly float Threshold;
        internal readonly string Parameter;

        public ACaaCParameterSingleCondition(AnimatorConditionMode mode, float threshold, string parameter)
        {
            Mode = mode;
            Threshold = threshold;
            Parameter = parameter;
        }
    }
}
