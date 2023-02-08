#if ANATAWA12_VRCSDK_AVATARS
using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using Anatawa12.AnimatorControllerAsACode.VRCAvatars3;
using UnityEngine;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Examples
{
    public class FacialExpression : GeneratorLayerBase
    {
        public Motion reset;
        public Motion fist;
        public Motion open;
        public Motion point;
        public Motion peace;
        public Motion rockNRoll;
        public Motion gun;
        public Motion thumbsUp;
        public Hand prior;

        protected override IEnumerable<Object> WatchingObjects => Array.Empty<Object>();

        protected override void Generate(Acc acc)
        {
            var lockedWeight = acc.FloatParameter($"LockedWeightConstant1").DefaultValue(1f);
            CreateResetFaceAlways(acc);
            CreateGestureLayer(acc, prior.Opposite(), lockedWeight);
            CreateResetFaceIfGesture(acc, prior);
            CreateGestureLayer(acc, prior, lockedWeight);
        }

        private void CreateResetFaceAlways(Acc acc)
        {
            var layer = acc.AddLayer("ResetAlways");
            var resetState = layer.NewState("Reset").WithAnimation(reset);
            layer.EntryTransitionsTo(resetState);
        }

        private void CreateResetFaceIfGesture(Acc acc, Hand side)
        {
            var layer = acc.AddLayer($"ResetIf{side}");
            var gesture = layer.Av3().Gesture(side);
            var lockFace = layer.BoolParameter("LockFace");

            var nopState = layer.NewState("Nop");
            var resetState = layer.NewState("Reset").WithAnimation(reset);

            layer.EntryTransitionsTo(nopState);
            nopState.TransitionsTo(resetState).When(gesture.IsNotEqualTo(0).And(lockFace.IsFalse()));
            resetState.TransitionsTo(nopState).When(gesture.IsEqualTo(0).And(lockFace.IsFalse()));
        }

        private void CreateGestureLayer(Acc acc, Hand side, AccParameter<float> lockedWeight)
        {
            var layer = acc.AddLayer($"{side}Hand");
            var lockFace = layer.BoolParameter("LockFace");
            var gesture = layer.Av3().Gesture(side);
            var weight = layer.Av3().GestureWeight(side);

            var stateIdle = layer.NewState("Idle");
            var stateFist = layer.NewState("Fist");
            var stateOpen = layer.NewState("Open");
            var statePoint = layer.NewState("Point");
            var statePeace = layer.NewState("Peace");
            var stateRockNRoll = layer.NewState("RockNRoll");
            var stateGun = layer.NewState("Gun");
            var stateThumbsUp = layer.NewState("ThumbsUp");

            var states = new[]
            {
                stateIdle,
                stateFist,
                stateOpen,
                statePoint,
                statePeace,
                stateRockNRoll,
                stateGun,
                stateThumbsUp,
            };

            // setup animations
            if (fist != null) stateFist.WithAnimation(fist);
            if (open != null) stateOpen.WithAnimation(open);
            if (point != null) statePoint.WithAnimation(point);
            if (peace != null) statePeace.WithAnimation(peace);
            if (rockNRoll != null) stateRockNRoll.WithAnimation(rockNRoll);
            if (gun != null) stateGun.WithAnimation(gun);
            if (thumbsUp != null) stateThumbsUp.WithAnimation(thumbsUp);

            stateIdle.TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Tracking);
            foreach (var state in states.Skip(1))
            {
                state.MotionTime(weight);
                state.TrackingSets(TrackingElement.Eyes, VRC_AnimatorTrackingControl.TrackingType.Animation);
            }

            // set transitions
            for (var i = 0; i < states.Length; i++)
            {
                layer.EntryTransitionsTo(states[i]).When(gesture.IsEqualTo((Gesture)i));
                states[i].TransitionsToExit().When(gesture.IsNotEqualTo((Gesture)i));
                var lockedState = layer.NewState($"{states[i].Name}Locked")
                    .RightOf(states[i])
                    .WithAnimation(states[i].Motion)
                    .MotionTime(lockedWeight);
                lockedState.SetAvatarParameter(lockedWeight, 1f);
                // due to VRChat's problem, this doesn't work.
                // see https://feedback.vrchat.com/avatar-30/p/copy-vrc-exposed-parameters-with-parameter-drivers
                // lockedState.CopyAvatarParameter(false, weight, lockedWeight);

                states[i].TransitionsTo(lockedState).When(lockFace.IsTrue());
                lockedState.TransitionsTo(states[i]).When(lockFace.IsFalse());
            }
        }
    }
}
#endif
