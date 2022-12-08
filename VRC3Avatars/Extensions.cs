using Anatawa12.AnimatorControllerAsACode.Generator;
using JetBrains.Annotations;

namespace Anatawa12.AnimatorControllerAsACode.VRCAvatars3
{
    public static class Extensions
    {
        public static Av3ParameterHolder Av3(this IACaaCParameterHolder self) => new Av3ParameterHolder(self);

        
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        public static Hand Opposite(this Hand hand) => hand ^ (Hand)1;
    }

    public readonly struct Av3ParameterHolder
    {
        [NotNull] private readonly IACaaCParameterHolder _holder;

        public Av3ParameterHolder([NotNull] IACaaCParameterHolder holder)
        {
            _holder = holder;
        }

        public ACaaCParameter<Gesture> GestureLeft => Gesture(Hand.Left);
        public ACaaCParameter<Gesture> GestureRight => Gesture(Hand.Right);
        public ACaaCParameter<Gesture> Gesture(Hand hand) => _holder.EnumParameter<Gesture>($"Gesture{hand}");

        public ACaaCParameter<float> GestureWeightLeft => GestureWeight(Hand.Left);
        public ACaaCParameter<float> GestureWeightRight => GestureWeight(Hand.Right);
        public ACaaCParameter<float> GestureWeight(Hand hand) => _holder.FloatParameter($"Gesture{hand}Weight");
    }

    public enum Gesture
    {
        // Specify all the values explicitly because they should be dictated by VRChat, not enumeration order.
        Neutral = 0,
        Fist = 1,
        HandOpen = 2,
        Fingerpoint = 3,
        Victory = 4,
        RockNRoll = 5,
        HandGun = 6,
        ThumbsUp = 7
    }

    public enum Hand
    {
        Left = 0,
        Right = 1,
    }
}
