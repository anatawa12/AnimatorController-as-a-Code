using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    internal static class Utils
    {
        public static void AddToFile(Object file, Object obj)
        {
            AssetDatabase.AddObjectToAsset(obj, file);
        }

        public static Func<float, float> FloatToFloat = x => x;
        public static Func<int, float> IntToFloat = x => x;
        public static Func<bool, float> BoolToFloat = x => x ? 1f : 0f;

        public static Func<T, float> EnumToFloat<T>()
            where T : unmanaged, Enum
            => EnumToFloatHelper<T>.ToFloat;

        private static class EnumToFloatHelper<T>
            where T : unmanaged, Enum
        {
            public static Func<T, float> ToFloat = Create();

            private static Func<T, float> Create()
            {
                switch (Unsafe.SizeOf<T>())
                {
                    case 1: return ForOneByte;
                    case 2: return ForTwoByte;
                    case 4: return ForFourByte;
                    case 8: return ForEightByte;
                    default: throw new ArgumentException("sizeof(T) is not 1, 2, 4, nor 8", nameof(T));
                }
            }

            private static float ForOneByte(T t) => Unsafe.As<T, byte>(ref t);
            private static float ForTwoByte(T t) => Unsafe.As<T, short>(ref t);
            private static float ForFourByte(T t) => Unsafe.As<T, int>(ref t);
            private static float ForEightByte(T t) => Unsafe.As<T, long>(ref t);
        }
    }
}
