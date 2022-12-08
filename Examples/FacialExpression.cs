using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using Anatawa12.AnimatorControllerAsACode.VRCAvatars3;
using UnityEngine;
using VRC.SDKBase;

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

        protected override void Generate(ACaaC acaac)
        {
            CreateResetFaceAlways(acaac);
            CreateGestureLayer(acaac, prior.Opposite());
            CreateResetFaceIfGesture(acaac, prior);
            CreateGestureLayer(acaac, prior);
        }

        private void CreateResetFaceAlways(ACaaC acaac)
        {
            var layer = acaac.AddLayer("ResetAlways");
            var resetState = layer.NewState("Reset").WithAnimation(reset);
            layer.EntryTransitionsTo(resetState);
        }

        private void CreateResetFaceIfGesture(ACaaC acaac, Hand side)
        {
            var layer = acaac.AddLayer($"ResetIf{side}");
            var gesture = layer.Av3().Gesture(side);
            var lockFace = layer.BoolParameter("LockFace");

            var nopState = layer.NewState("Nop");
            var resetState = layer.NewState("Reset").WithAnimation(reset);

            layer.EntryTransitionsTo(nopState);
            nopState.TransitionsTo(resetState).When(gesture.IsNotEqualTo(Gesture.Neutral));
            resetState.TransitionsTo(nopState).When(gesture.IsEqualTo(Gesture.Neutral));
            nopState.TransitionsTo(resetState).When(gesture.IsNotEqualTo(0).And(lockFace.IsTrue()));
            resetState.TransitionsTo(nopState).When(gesture.IsEqualTo(0).And(lockFace.IsTrue()));
        }

        private void CreateGestureLayer(ACaaC acaac, Hand side)
        {
            var layer = acaac.AddLayer($"{side}Hand");
            var lockFace = layer.BoolParameter("LockFace");
            var gesture = layer.Av3().Gesture(side);
            var weight = layer.Av3().GestureWeight(side);

            var stateIdle = layer.NewState("Idle");
            var stateFist = layer.NewState("Fist").RightOf();
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
            layer.EntryTransitionsTo(stateIdle);

            for (var i = 0; i < states.Length; i++)
            {
                layer.AnyTransitionsTo(states[i])
                    .WithTransitionDurationSeconds(0.1f)
                    .When(gesture.IsEqualTo((Gesture)i).And(lockFace.IsFalse()));
            }
        }
    }
}
