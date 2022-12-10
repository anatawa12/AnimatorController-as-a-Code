using System;
using Anatawa12.AnimatorControllerAsACode.Framework;
using JetBrains.Annotations;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Anatawa12.AnimatorControllerAsACode.VRCAvatars3
{
    public static class Extensions
    {
        public static Av3ParameterHolder Av3(this IACaaCParameterHolder self) => new Av3ParameterHolder(self);

        public static void TrackingSets(this ACaaCState self, TrackingElement element, VRC_AnimatorTrackingControl.TrackingType target)
        {
            var tracking = self.AddOrFindStateMachineBehaviour<VRCAnimatorTrackingControl>();
            
            switch (element)
            {
                case TrackingElement.Head:
                    tracking.trackingHead = target;
                    break;
                case TrackingElement.LeftHand:
                    tracking.trackingLeftHand = target;
                    break;
                case TrackingElement.RightHand:
                    tracking.trackingRightHand = target;
                    break;
                case TrackingElement.Hip:
                    tracking.trackingHip = target;
                    break;
                case TrackingElement.LeftFoot:
                    tracking.trackingLeftFoot = target;
                    break;
                case TrackingElement.RightFoot:
                    tracking.trackingRightFoot = target;
                    break;
                case TrackingElement.LeftFingers:
                    tracking.trackingLeftFingers = target;
                    break;
                case TrackingElement.RightFingers:
                    tracking.trackingRightFingers = target;
                    break;
                case TrackingElement.Eyes:
                    tracking.trackingEyes = target;
                    break;
                case TrackingElement.Mouth:
                    tracking.trackingMouth = target;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(element), element, "invalid TrackingElement");
            }
        }
        
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

    public enum TrackingElement
    {
        Head,
        LeftHand,
        RightHand,
        Hip,
        LeftFoot,
        RightFoot,
        LeftFingers,
        RightFingers,
        Eyes,
        Mouth
    }
}
