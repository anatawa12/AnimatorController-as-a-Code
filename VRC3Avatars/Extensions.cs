using System;
using Anatawa12.AnimatorControllerAsACode.Framework;
using JetBrains.Annotations;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace Anatawa12.AnimatorControllerAsACode.VRCAvatars3
{
    public static class Extensions
    {
        public static Av3ParameterHolder Av3(this IACCParameterHolder self) => new Av3ParameterHolder(self);

        public static void TrackingSets(this AccState self, TrackingElement element, VRC_AnimatorTrackingControl.TrackingType target)
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

        #region VRCAvatarParameterDriver

        private static void AddAvatarParameterDriverParameter(this AccState self, bool localOnly, VRC_AvatarParameterDriver.Parameter parameter)
        {
            var parameterDriver =
                self.FindStateMachineBehaviour<VRCAvatarParameterDriver>(x => x.localOnly == localOnly)
                ?? self.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            parameterDriver.localOnly = localOnly;

            parameterDriver.parameters.Add(parameter);
        }

        private static void SetAvatarParameter(this AccState self, bool localOnly, string parameter, float value) =>
            self.AddAvatarParameterDriverParameter(localOnly, new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = parameter,
                value = value,
            });

        public static void SetAvatarParameter<T>(this AccState self, AccParameter<T> parameter, T value)
            where T : unmanaged => SetAvatarParameter(self, false, parameter.Name, parameter.ToFloat(value));

        public static void SetAvatarParameterLocally<T>(this AccState self, AccParameter<T> parameter, T value)
            where T : unmanaged => SetAvatarParameter(self, true, parameter.Name, parameter.ToFloat(value));

        private static void AddAvatarParameter(this AccState self, bool localOnly, string parameter, float value) =>
            self.AddAvatarParameterDriverParameter(localOnly, new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Add,
                name = parameter,
                value = value,
            });

        public static void AddAvatarParameter(this AccState self, AccParameter<float> parameter, float value) =>
            AddAvatarParameter(self, false, parameter.Name, value);

        public static void AddAvatarParameter(this AccState self, AccParameter<int> parameter, int value) =>
            AddAvatarParameter(self, false, parameter.Name, value);

        public static void AddAvatarParameterLocally(this AccState self, AccParameter<float> parameter, float value) =>
            AddAvatarParameter(self, true, parameter.Name, value);

        public static void AddAvatarParameterLocally(this AccState self, AccParameter<int> parameter, int value) =>
            AddAvatarParameter(self, true, parameter.Name, value);

        private static void SetAvatarParameterToRandom(this AccState self, bool localOnly, string parameter, float min, float max) =>
            self.AddAvatarParameterDriverParameter(localOnly, new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Random,
                name = parameter,
                valueMin = min,
                valueMax = max,
            });

        private static void SetAvatarParameterToRandomBool(this AccState self, bool localOnly, string parameter, float chance) =>
            self.AddAvatarParameterDriverParameter(localOnly, new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Random,
                name = parameter,
                chance = chance,
            });

        [Obsolete("Because generated random value is different among clients, It's not recommended to use local & sync. " 
            + "If you actually want to generate random value in each client, use SetAvatarParameterToRandomOnEachClient")]
        public static void SetAvatarParameterToRandom(this AccState self, AccParameter<float> parameter, float min, float max) =>
            SetAvatarParameterToRandomOnEachClient(self, parameter, min, max);

        [Obsolete("Because generated random value is different among clients, It's not recommended to use local & sync. " 
                  + "If you actually want to generate random value in each client, use SetAvatarParameterToRandomOnEachClient")]
        public static void SetAvatarParameterToRandom(this AccState self, AccParameter<int> parameter, int min, int max) =>
            SetAvatarParameterToRandomOnEachClient(self, parameter, min, max);

        /// <param name="self">this object</param>
        /// <param name="parameter">the bool parameter</param>
        /// <param name="chance">the chance of true. (1-chance) of chane for false.</param>
        [Obsolete("Because generated random value is different among clients, It's not recommended to use local & sync. " 
                  + "If you actually want to generate random value in each client, use SetAvatarParameterToRandomOnEachClient")]
        public static void SetAvatarParameterToRandom(this AccState self, AccParameter<bool> parameter, float chance) =>
            SetAvatarParameterToRandomOnEachClient(self, parameter, chance);

        public static void SetAvatarParameterToRandomOnEachClient(this AccState self, AccParameter<float> parameter, float min, float max) =>
            SetAvatarParameterToRandom(self, false, parameter.Name, min, max);

        public static void SetAvatarParameterToRandomOnEachClient(this AccState self, AccParameter<int> parameter, int min, int max) =>
            SetAvatarParameterToRandom(self, false, parameter.Name, min, max);

        /// <param name="self">this object</param>
        /// <param name="parameter">the bool parameter</param>
        /// <param name="chance">the chance of true. (1-chance) of chane for false.</param>
        public static void SetAvatarParameterToRandomOnEachClient(this AccState self, AccParameter<bool> parameter, float chance) =>
            SetAvatarParameterToRandomBool(self, false, parameter.Name, chance);

        public static void SetAvatarParameterToRandomLocally(this AccState self, AccParameter<float> parameter, float min, float max) =>
            SetAvatarParameterToRandom(self, true, parameter.Name, min, max);

        public static void SetAvatarParameterToRandomLocally(this AccState self, AccParameter<int> parameter, int min, int max) =>
            SetAvatarParameterToRandom(self, true, parameter.Name, min, max);

        /// <param name="self">this object</param>
        /// <param name="parameter">the bool parameter</param>
        /// <param name="chance">the chance of true. (1-chance) of chane for false.</param>
        public static void SetAvatarParameterToRandomLocally(this AccState self, AccParameter<bool> parameter, float chance) =>
            SetAvatarParameterToRandomBool(self, true, parameter.Name, chance);

        private static void CopyAvatarParameter(this AccState self, bool localOnly, string source, string dest) =>
            self.AddAvatarParameterDriverParameter(localOnly, new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Copy,
                source = source,
                name = dest,
            });

        public static void CopyAvatarParameter<T1, T2>(this AccState self, bool localOnly, AccParameter<T1> source,
            AccParameter<T2> dest) where T1 : unmanaged where T2 : unmanaged =>
            CopyAvatarParameter(self, localOnly, source.Name, dest.Name);

        private static void CopyAvatarParameter(this AccState self, bool localOnly, 
            string source, float sourceMin, float sourceMax, 
            string dest,　float destMin, float destMax) =>
            self.AddAvatarParameterDriverParameter(localOnly, new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Copy,
                source = source,
                name = dest,
                convertRange = true,
                sourceMin = sourceMin,
                sourceMax = sourceMax,
                destMin = destMin,
                destMax = destMax,
            });

        // between numeric: just a mapping
        public static void CopyAvatarParameter(this AccState self, bool localOnly, 
            AccParameter<float> source, float sourceMin, float sourceMax, 
            AccParameter<float> dest,　float destMin, float destMax) =>
            CopyAvatarParameter(self, localOnly, source.Name, sourceMin, sourceMax, dest.Name, destMin, destMax);

        public static void CopyAvatarParameter(this AccState self, bool localOnly, 
            AccParameter<float> source, float sourceMin, float sourceMax, 
            AccParameter<int> dest,　int destMin, int destMax) =>
            CopyAvatarParameter(self, localOnly, source.Name, sourceMin, sourceMax, dest.Name, destMin, destMax);

        public static void CopyAvatarParameter(this AccState self, bool localOnly,
            AccParameter<int> source,　int sourceMin, int sourceMax,
            AccParameter<float> dest,　float destMin, float destMax) =>
            CopyAvatarParameter(self, localOnly, source.Name, sourceMin, sourceMax, dest.Name, destMin, destMax);

        public static void CopyAvatarParameter(this AccState self, bool localOnly,
            AccParameter<int> source,　int sourceMin, int sourceMax,
            AccParameter<int> dest,　int destMin, int destMax) =>
            CopyAvatarParameter(self, localOnly, source.Name, sourceMin, sourceMax, dest.Name, destMin, destMax);

        // bool -> numeric
        public static void ToggleAvatarParameter(this AccState self, bool localOnly, 
            AccParameter<bool> source, AccParameter<float> dest,    float ifFalse, float ifTrue) =>
            CopyAvatarParameter(self, localOnly, source.Name, 0f, 1f, dest.Name, ifFalse, ifTrue);

        public static void ToggleAvatarParameter(this AccState self, bool localOnly, 
            AccParameter<bool> source, AccParameter<int> dest,    int ifFalse, int ifTrue) =>
            CopyAvatarParameter(self, localOnly, source.Name, 0f, 1f, dest.Name, ifFalse, ifTrue);

        // numeric -> bool is too small range of usage so I don't want to support it
        
        #endregion VRCAvatarParameterDriver

        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        public static Hand Opposite(this Hand hand) => hand ^ (Hand)1;
    }

    public readonly struct Av3ParameterHolder
    {
        [NotNull] private readonly IACCParameterHolder _holder;

        public Av3ParameterHolder([NotNull] IACCParameterHolder holder)
        {
            _holder = holder;
        }

        // see https://docs.vrchat.com/docs/animator-parameters

        public AccParameter<bool> IsLocal => _holder.BoolParameter("IsLocal");

        public AccParameter<OculusViseme> OculusViseme => _holder.EnumParameter<OculusViseme>("Viseme");
        public AccParameter<int> JarViseme => _holder.IntParameter("Viseme");
        public AccParameter<float> Voice => _holder.FloatParameter("Voice");

        public AccParameter<Gesture> GestureLeft => Gesture(Hand.Left);
        public AccParameter<Gesture> GestureRight => Gesture(Hand.Right);
        public AccParameter<Gesture> Gesture(Hand hand) => _holder.EnumParameter<Gesture>($"Gesture{hand}");

        public AccParameter<float> GestureWeightLeft => GestureWeight(Hand.Left);
        public AccParameter<float> GestureWeightRight => GestureWeight(Hand.Right);
        public AccParameter<float> GestureWeight(Hand hand) => _holder.FloatParameter($"Gesture{hand}Weight");

        public AccParameter<float> AngularY => _holder.FloatParameter("AngularY");
        
        public AccParameter<float> VelocityX => _holder.FloatParameter("VelocityX");
        public AccParameter<float> VelocityY => _holder.FloatParameter("VelocityY");
        public AccParameter<float> VelocityZ => _holder.FloatParameter("VelocityZ");

        public AccParameter<float> Upright => _holder.FloatParameter("Upright");
        public AccParameter<bool> Grounded => _holder.BoolParameter("Grounded");
        public AccParameter<bool> Seated => _holder.BoolParameter("Seated");
        public AccParameter<bool> Afk => _holder.BoolParameter("AFK");

        // Expression{1,16} are not recommended

        // TODO
        public AccParameter<int> TrackingType => _holder.IntParameter("TrackingType");
        public AccParameter<VRMode> VRMode => _holder.EnumParameter<VRMode>("VRMode");
        public AccParameter<bool> MuteSelf => _holder.BoolParameter("MuteSelf");
        public AccParameter<bool> InStation => _holder.BoolParameter("InStation");
        public AccParameter<bool> Earmuffs => _holder.BoolParameter("Earmuffs");
    }

    public enum OculusViseme
    {
        Sil = 0,
        Pp = 1,
        Ff = 2,
        Th = 3,
        Dd = 4,
        Kk = 5,
        Ch = 6,
        Ss = 7,
        Nn = 8,
        Rr = 9,
        Aa = 10,
        E = 11,
        I = 12,
        O = 13,
        U = 14,
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

    public enum TrackingType
    {
        Uninitialized = 0,
        GenericRig = 1,
        // 2: Hands only tracking but only for AV2
        HeadAndHand = 3,
        HeadHandsAndHip = 4,
        FullBody = 6,
    }

    public enum VRMode
    {
        Desktop = 0,
        VR = 1,
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
