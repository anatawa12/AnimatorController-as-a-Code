using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public class ACCParameter<T>
    {
        private readonly AnimatorControllerParameter _parameter;
        private readonly Func<T, float> _toFloat;

        public ACCParameter(AnimatorControllerParameter parameter, Func<T, float> toFloat)
        {
            _parameter = parameter;
            _toFloat = toFloat;
        }

        public string Name => _parameter.name;

        public ACCParameterCondition IsEqualTo(T value)
        {
            return new ACCParameterCondition(
                new ACCParameterSingleCondition(AnimatorConditionMode.Equals, _toFloat(value), Name));
        }

        public ACCParameterCondition IsNotEqualTo(T value)
        {
            return new ACCParameterCondition(
                new ACCParameterSingleCondition(AnimatorConditionMode.NotEqual, _toFloat(value), Name));
        }
    }

    public static class ACCTypeSpecificMethods
    {
        public static ACCParameterCondition IsFalse(this ACCParameter<bool> self) => self.IsEqualTo(false);
        public static ACCParameterCondition IsTrue(this ACCParameter<bool> self) => self.IsEqualTo(true);
    }

    public readonly struct ACCParameterCondition
    {
        private readonly ACCParameterSingleCondition[] _conditions;

        internal ACCParameterCondition(ACCParameterSingleCondition condition)
        {
            _conditions = new[]
            {
                condition
            };
        }

        private ACCParameterCondition(ACCParameterSingleCondition[] conditions)
        {
            _conditions = conditions;
        }

        public ACCParameterCondition And(ACCParameterCondition other)
        {
            if (other._conditions == null || other._conditions.Length == 0)
                return this;
            if (_conditions == null || _conditions.Length == 0)
                return other;
            var result = new ACCParameterSingleCondition[_conditions.Length + other._conditions.Length];
            Array.Copy(_conditions, result, _conditions.Length);
            Array.Copy(other._conditions, 0, result, _conditions.Length, other._conditions.Length);
            return new ACCParameterCondition(result);
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

    internal readonly struct ACCParameterSingleCondition
    {
        internal readonly AnimatorConditionMode Mode;
        internal readonly float Threshold;
        internal readonly string Parameter;

        public ACCParameterSingleCondition(AnimatorConditionMode mode, float threshold, string parameter)
        {
            Mode = mode;
            Threshold = threshold;
            Parameter = parameter;
        }
    }
}
