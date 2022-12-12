using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Framework.Clip
{
    internal interface ISettingCurveTypeInfo<in T, out TBindingArray, out TFloatArray, out TKeyframeListArray>
        where TBindingArray : struct, IFixedArray<EditorCurveBinding>
        where TFloatArray : struct, IFixedArray<float>
        where TKeyframeListArray : struct, IFixedArray<List<Keyframe>>
    {
        TBindingArray CreateBindings(EditorCurveBinding binding);
        TKeyframeListArray CreateKeyframeLists();
        TFloatArray ToFloats(T value);
    }

    internal struct IntTypeInfo : ISettingCurveTypeInfo<int, Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>>
    {
        public Fixed1<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) => binding;

        public Fixed1<List<Keyframe>> CreateKeyframeLists() => new List<Keyframe>();

        public Fixed1<float> ToFloats(int value) => value;
    }

    internal struct BoolTypeInfo : ISettingCurveTypeInfo<bool, Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>>
    {
        public Fixed1<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) => binding;

        public Fixed1<List<Keyframe>> CreateKeyframeLists() => new List<Keyframe>();

        public Fixed1<float> ToFloats(bool value) => value ? 1f : 0f;
    }

    internal struct FloatTypeInfo : ISettingCurveTypeInfo<float, Fixed1<EditorCurveBinding>, Fixed1<float>,
        Fixed1<List<Keyframe>>>
    {
        public Fixed1<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) => binding;

        public Fixed1<List<Keyframe>> CreateKeyframeLists() => new List<Keyframe>();

        public Fixed1<float> ToFloats(float value) => value;
    }

    internal struct EnumTypeInfo<T> : ISettingCurveTypeInfo<T, Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>>

    {
        public Fixed1<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) => binding;

        public Fixed1<List<Keyframe>> CreateKeyframeLists() => new List<Keyframe>();

        public Fixed1<float> ToFloats(T value) => (float)Unsafe.As<T, int>(ref value);
    }

    internal struct ColorTypeInfo : ISettingCurveTypeInfo<Color, Fixed4<EditorCurveBinding>, Fixed4<float>,
        Fixed4<List<Keyframe>>>
    {
        public Fixed4<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "r"),
                Utils.SubBinding(binding, "g"),
                Utils.SubBinding(binding, "b"),
                Utils.SubBinding(binding, "a"));

        public Fixed4<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed4<float> ToFloats(Color value) => (value.r, value.g, value.b, value.a);
    }

    internal struct Vector2TypeInfo : ISettingCurveTypeInfo<Vector2, Fixed2<EditorCurveBinding>, Fixed2<float>,
        Fixed2<List<Keyframe>>>
    {
        public Fixed2<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"));

        public Fixed2<List<Keyframe>> CreateKeyframeLists() => (new List<Keyframe>(), new List<Keyframe>());

        public Fixed2<float> ToFloats(Vector2 value) => (value.x, value.y);
    }

    internal struct Vector2IntTypeInfo : ISettingCurveTypeInfo<Vector2Int, Fixed2<EditorCurveBinding>, Fixed2<float>,
        Fixed2<List<Keyframe>>>
    {
        public Fixed2<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"));

        public Fixed2<List<Keyframe>> CreateKeyframeLists() => (new List<Keyframe>(), new List<Keyframe>());

        public Fixed2<float> ToFloats(Vector2Int value) => (value.x, value.y);
    }

    internal struct Vector3TypeInfo : ISettingCurveTypeInfo<Vector3, Fixed3<EditorCurveBinding>, Fixed3<float>,
        Fixed3<List<Keyframe>>>
    {
        public Fixed3<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"),
                Utils.SubBinding(binding, "z"));

        public Fixed3<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed3<float> ToFloats(Vector3 value) => (value.x, value.y, value.z);
    }

    internal struct Vector3IntTypeInfo : ISettingCurveTypeInfo<Vector3Int, Fixed3<EditorCurveBinding>, Fixed3<float>,
        Fixed3<List<Keyframe>>>
    {
        public Fixed3<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"),
                Utils.SubBinding(binding, "z"));

        public Fixed3<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed3<float> ToFloats(Vector3Int value) => (value.x, value.y, value.z);
    }

    internal struct Vector4TypeInfo : ISettingCurveTypeInfo<Vector4, Fixed4<EditorCurveBinding>, Fixed4<float>,
        Fixed4<List<Keyframe>>>
    {
        public Fixed4<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"),
                Utils.SubBinding(binding, "z"),
                Utils.SubBinding(binding, "w"));

        public Fixed4<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed4<float> ToFloats(Vector4 value) => (value.x, value.y, value.z, value.w);
    }

    internal struct QuaternionTypeInfo : ISettingCurveTypeInfo<Quaternion, Fixed4<EditorCurveBinding>, Fixed4<float>,
        Fixed4<List<Keyframe>>>
    {
        public Fixed4<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"),
                Utils.SubBinding(binding, "z"),
                Utils.SubBinding(binding, "w"));

        public Fixed4<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed4<float> ToFloats(Quaternion value) => (value.x, value.y, value.z, value.w);
    }

    internal struct RectTypeInfo : ISettingCurveTypeInfo<Rect, Fixed4<EditorCurveBinding>, Fixed4<float>, Fixed4<List<Keyframe>>>
    {
        public Fixed4<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"),
                Utils.SubBinding(binding, "height"),
                Utils.SubBinding(binding, "width"));

        public Fixed4<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed4<float> ToFloats(Rect value) => (value.x, value.y, value.height, value.width);
    }

    internal struct RectIntTypeInfo : ISettingCurveTypeInfo<RectInt, Fixed4<EditorCurveBinding>, Fixed4<float>,
        Fixed4<List<Keyframe>>>
    {
        public Fixed4<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "x"),
                Utils.SubBinding(binding, "y"),
                Utils.SubBinding(binding, "height"),
                Utils.SubBinding(binding, "width"));

        public Fixed4<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>());

        public Fixed4<float> ToFloats(RectInt value) => (value.x, value.y, value.height, value.width);
    }

    internal struct BoundsTypeInfo : ISettingCurveTypeInfo<Bounds, Fixed6<EditorCurveBinding>, Fixed6<float>,
        Fixed6<List<Keyframe>>>
    {
        public Fixed6<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "m_Center.x"),
                Utils.SubBinding(binding, "m_Center.y"),
                Utils.SubBinding(binding, "m_Center.z"),
                Utils.SubBinding(binding, "m_Extent.x"),
                Utils.SubBinding(binding, "m_Extent.y"),
                Utils.SubBinding(binding, "m_Extent.z"));

        public Fixed6<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(),
                new List<Keyframe>(), new List<Keyframe>());

        public Fixed6<float> ToFloats(Bounds value) => (value.center.x, value.center.y, value.center.z,
            value.extents.x, value.extents.y, value.extents.z);
    }

    internal struct BoundsIntTypeInfo : ISettingCurveTypeInfo<BoundsInt, Fixed6<EditorCurveBinding>, Fixed6<float>,
        Fixed6<List<Keyframe>>>
    {
        public Fixed6<EditorCurveBinding> CreateBindings(EditorCurveBinding binding) =>
            (Utils.SubBinding(binding, "m_Position.x"),
                Utils.SubBinding(binding, "m_Position.y"),
                Utils.SubBinding(binding, "m_Position.z"),
                Utils.SubBinding(binding, "m_Size.x"),
                Utils.SubBinding(binding, "m_Size.y"),
                Utils.SubBinding(binding, "m_Size.z"));

        public Fixed6<List<Keyframe>> CreateKeyframeLists() =>
            (new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(), new List<Keyframe>(),
                new List<Keyframe>(), new List<Keyframe>());

        public Fixed6<float> ToFloats(BoundsInt value) => (value.position.x, value.position.y, value.position.z,
            value.size.x, value.size.y, value.size.z);
    }
}
