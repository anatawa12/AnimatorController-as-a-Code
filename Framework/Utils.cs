using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Framework
{
    internal static class Utils
    {
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
    }
}
