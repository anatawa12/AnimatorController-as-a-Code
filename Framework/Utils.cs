using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    internal static class Utils
    {
        private static Lazy<AnimationClip> _clip =
            new Lazy<AnimationClip>(() => LoadAssetGuid<AnimationClip>("c4de39b9a1ef640a6aefe68923bb35a9"));

        public static T LoadAssetGuid<T>(string guid) where T : Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == null) return null;
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static AnimationClip GetEmptyClip() => _clip.Value;

        // .NET 4.x which is used in unity doesn't have Deconstruct method so I declare here
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value)
        {
            key = self.Key;
            value = self.Value;
        }

        public static void AddToFile(Object file, Object obj)
        {
            AssetDatabase.AddObjectToAsset(obj, file);
        }

        public static readonly Func<float, float> FloatToFloat = x => x;
        public static readonly Func<int, float> IntToFloat = x => x;
        public static readonly Func<bool, float> BoolToFloat = x => x ? 1f : 0f;

        // Animation only supports int-based enums.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, float> EnumToFloat<T>()
            where T : unmanaged, Enum
            => EnumToFloatHelper<T>.ToFloat ?? ThrowEnumUnderlyingTypeError<T>();

        private static Func<T, float> ThrowEnumUnderlyingTypeError<T>() =>
            throw new ArgumentException("UnderlyingType of T is not int. Animating such a field is not supported.",
                nameof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T CheckedIndexing<T>(ref T first, int index, int len)
        {
            CheckIndex(index, len);
            return ref Unsafe.Add(ref first, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckIndex(int index, int len)
        {
            if (index < 0) ThrowIndexError();
            if (index >= len) ThrowIndexError();
        }

        private static void ThrowIndexError()
        {
            throw new IndexOutOfRangeException("index out of bounds");
        }

        private static class EnumToFloatHelper<T>
            where T : unmanaged, Enum
        {
            public static readonly Func<T, float> ToFloat = Create();

            private static Func<T, float> Create()
            {
                if (Enum.GetUnderlyingType(typeof(T)) == typeof(int))
                    return t => Unsafe.As<T, int>(ref t);
                return null;
            }
        }

        public static EditorCurveBinding SubBinding(EditorCurveBinding binding, string prop)
        {
            return new EditorCurveBinding
            {
                path = binding.path,
                type = binding.type,
                propertyName = binding.propertyName + "." + prop,
            };
        }

        public static T[] JoinArray<T, T1>(T[] controllerLayers, List<T1> addingLayers, Func<T1, T> func)
        {
            var offset = controllerLayers.Length;
            Array.Resize(ref controllerLayers, controllerLayers.Length + addingLayers.Count);
            for (var i = 0; i < addingLayers.Count; i++)
                controllerLayers[offset + i] = func(addingLayers[i]);
            return controllerLayers;
        }
    }
}
