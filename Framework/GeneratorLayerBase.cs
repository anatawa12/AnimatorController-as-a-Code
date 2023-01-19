using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    public abstract class GeneratorLayerBase : ScriptableObject
    {
#if DOC_LANG_JA
        /// <summary>
        /// ジェネレーターレイヤーのデフォルトの名前。これはアニメーターレイヤーの接頭辞として使用されます。
        /// デフォルトではクラス名が使用されます。
        /// アニメーターレイヤーの名前の競合を避けるために'_'を使用しないでください。
        /// </summary>
#else
        /// <summary>
        /// The default name of Generator Layer. This is used as a prefix of animator layers.
        /// By default, name of class is used.
        /// This name should not contain '_' to avoid conflict.
        /// </summary>
#endif
        protected internal virtual string DefaultName => NormalizeGeneratorDefaultName(GetType().Name);

#if DOC_LANG_JA
        /// <summary>
        /// ウォッチング中のオブジェクトのリスト。
        /// </summary>
        /// <remarks>
        /// このリストにあっセットを追加するのを忘れないでください。
        /// もし追加し忘れると自動再生成が正しく動かないです。
        /// もし余計なアセットをこのすリストに追加するのは問題ないです。
        /// この場合、必要以上に自動再生成が行われますが何も壊れません。
        ///
        /// もしあなたの実装がアセットの一部を<see cref="AnimatorController"/>に<b>コピー</b>したり<b>使用</b>する場合、
        /// このリストにそのアセットを含めなければなりません。もし忘れると自動再生成が正しく動かないです。
        ///
        /// もしあなたの実装が<see cref="AnimatorController"/>にアセットを<b>参照</b>させる場合、
        /// このリストにそのアセットを含める必要はありません。
        /// </remarks>
        /// <example>
        /// <c>CopyFromOtherControllerLayer</c> (example モジュール内) は<c>controller.layers</c>の一部をコピーするため、
        /// <c>controller</c>を<c>WatchingObjects</c>に追加します。
        /// </example>
        /// <example>
        /// <c>FacialExpression</c> (example モジュール内) は<c>Motion</c>をコピーや使用しません。
        /// そのため、<c>WatchingObjects</c>に<c>motions</c>を追加しません。
        /// </example>
#else
        /// <summary>
        /// The list of watching objects.
        /// </summary>
        /// <remarks>
        /// Don't forget to add some assets to this list.
        /// If you forget, auto regeneration do not work as expected.
        /// If you added too many assets to this list, It's OK.
        /// it will make more regeneration than required but nothing will be broken.
        ///
        /// If your implementation <b>copies</b> or <b>uses</b> some part of asset to AnimatorController,
        /// You <b>must</b> include the asset to this list. If you forget, automatic generation will not work as expect
        ///
        /// If your implementation just make generated AnimatorController <b>reference</b> some asset,
        /// You <b>don't have to </b> include the asset to this list.
        /// </remarks>
        /// <example>
        /// <c>CopyFromOtherControllerLayer</c> (in example module) copies some part of <c>controller.layers</c>
        /// so it adds <c>controller</c> to <c>WatchingObjects</c>.
        /// </example>
        /// <example>
        /// <c>FacialExpression</c> (in example module) do not copies nor uses part of <c>Motion</c>
        /// so It doesn't add motions to <c>WatchingObjects</c>.
        /// </example>
#endif
        protected internal abstract IEnumerable<Object> WatchingObjects { get; }

        // backing field of Generator reference
        [SerializeField]
        [HideInInspector]
        [Obsolete("you should not access this field directly", true)]
        internal Object generator;

        /// <summary>
        /// Generates Animator Layers.
        /// </summary>
        /// <param name="acc">Animator As A Code entrypoint object</param>
        protected internal abstract void Generate(Acc acc);

        public static string NormalizeGeneratorDefaultName(string name)
        {
            name = name.Replace("_", "");
            if (name.Length != 0 && '0' <= name[name.Length - 1] && name[name.Length - 1] <= '9')
                name = name.Substring(0, name.Length - 1);
            if (name.Length == 0) name = "unnamed";
            return name;
        }

        private void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
        }
    }
}
