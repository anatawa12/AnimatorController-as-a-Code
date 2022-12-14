using System.Linq;
using Anatawa12.AnimatorControllerAsACode.Framework;
using Anatawa12.AnimatorControllerAsACode.VRCAvatars3;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Examples
{
    /// <summary>
    /// This generates animator to sync k-bit integer from k of bool.
    /// On local, parameter named 'intParameter' is converted to k of bool ($"{backingParameterPrefix}{i}")
    /// and On remote, combines k of bool to one int.
    ///
    /// **WARNING**
    /// I found using this generator is slow if you're using big bits.
    /// For 8-bit, only with this generator, generated controller will be about 4.6 MB.
    /// PLEASE BE CAREFUL.
    ///
    /// This is workaround until k-bit integer sync parameter is added to VRChat.
    /// Here's canny about k-bit integer: https://feedback.vrchat.com/avatar-30/p/feedback-specify-amount-of-bits-for-our-integer-parameters.
    /// </summary>
    public class KBitInteger : GeneratorLayerBase
    {
        public string intParameter;
        public string backingParameterPrefix;

        // TODO: support up to 24 bit (float-representable maximum) integer?
        [Range(1, 24)]
        public int bits = 1;

        protected override void Generate(Acc acc)
        {
            var intParam = acc.IntParameter(intParameter);
            var boolParams = Enumerable.Range(0, bits)
                .Select(i => acc.BoolParameter($"{backingParameterPrefix}{i}"))
                .ToArray();

            var layer = acc.AddMainLayer();

            var receiver = layer.NewSubStateMachine("Receiver");
            CreateReceiver(receiver, intParam, boolParams, acc.Av3().IsLocal);
            
            var sender = layer.NewSubStateMachine("Sender");
            CreateSender(sender, intParam, boolParams);

            layer.EntryTransitionsTo(receiver);
            receiver.TransitionsTo(sender);
        }

        private void CreateReceiver(AccStateMachine receiver, AccParameter<int> intParam, AccParameter<bool>[] boolParams, AccParameter<bool> isLocal)
        {
            var count = 1 << bits;
            var rComputing = 1 + count / 6f;
            var rSetting = 2 + count / 6f;
            
            var computingStates = Enumerable.Range(0, count)
                .Select(i => receiver.NewState($"ReceiverComputing0x{i:x2}"))
                .ToArray();
            var settingStates = Enumerable.Range(0, count)
                .Select(i => receiver.NewState($"ReceiverSetting0x{i:x2}"))
                .ToArray();

            for (var i = 0; i < count; i++)
            {
                var currentComputing = computingStates[i];
                var currentSettingState = settingStates[i];
                for (var bitNum = 0; bitNum < bits; bitNum++)
                {
                    var bit = 1 << bitNum;
                    var inverted = i ^ bit;
                    var targetComputing = computingStates[inverted];
                    var currentBit = (i & bit) != 0;
                    currentSettingState.TransitionsTo(targetComputing)
                        .When(boolParams[bitNum].IsNotEqualTo(currentBit));
                    currentComputing.TransitionsTo(targetComputing)
                        .When(boolParams[bitNum].IsNotEqualTo(currentBit));
                }

                currentComputing.TransitionsTo(currentSettingState);
                currentSettingState.SetAvatarParameter(intParam, i);

                var xAngle = Mathf.Cos(i / (float)count * Mathf.PI * 2);
                var yAngle = Mathf.Sin(i / (float)count * Mathf.PI * 2);
                currentComputing.OnGridAt(xAngle * rComputing, yAngle * rComputing);
                currentSettingState.OnGridAt(xAngle * rSetting, yAngle * rSetting);

                currentSettingState.TransitionsToExit()
                    .When(isLocal.IsTrue());
            }

            receiver.EntryTransitionsTo(computingStates[0]);
        }
        
        private void CreateSender(AccStateMachine sender, AccParameter<int> intParam, AccParameter<bool>[] boolParams)
        {
            var senderEntry = sender.NewState("SenderEntryPoint").OnGridAt(0, 0);

            var count = 1 << bits;
            var rComputing = 1 + count / 6f;

            var setting = new AccState[count];
            for (var i = 0; i < setting.Length; i++)
            {
                var state = setting[i] = sender.NewState($"SenderSet0x{i:x2}")
                    .Offset(senderEntry, 1, i);
                for (var bitNum = 0; bitNum < bits; bitNum++)
                    state.SetAvatarParameterLocally(boolParams[bitNum], (i & (1 << bitNum)) != 0);
                senderEntry.TransitionsTo(state).When(intParam.IsEqualTo(i));
                state.TransitionsTo(senderEntry).When(intParam.IsNotEqualTo(i));
                
                var xAngle = Mathf.Cos(i / (float)count * Mathf.PI * 2);
                var yAngle = Mathf.Sin(i / (float)count * Mathf.PI * 2);
                state.OnGridAt(xAngle * rComputing, yAngle * rComputing);
            }
        }
    }
}
