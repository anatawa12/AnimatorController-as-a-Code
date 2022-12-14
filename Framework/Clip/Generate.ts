function commaSeparated(n: number, f: (i: number) => string): string {
    let result = "";
    for (let i = 0; i < n; i++) {
        if (i != 0) result += ", ";
        result += f(i);
    }
    return result
}

console.log("// generated by Generate.ts")
console.log("// running Generate.ts via deno will output this file.")
console.log("")
console.log("using System;")
console.log("using System.Collections.Generic;")
console.log("using System.Linq.Expressions;")
console.log("using System.Runtime.CompilerServices;")
console.log("using Anatawa12.AnimatorControllerAsACode.Framework.Clip;")
console.log("using UnityEditor;")
console.log("using UnityEngine;")
console.log("")
console.log("#pragma warning disable CS0649")
console.log("")
// fixedArrays
console.log("namespace Anatawa12.AnimatorControllerAsACode.Framework.Clip")
console.log("{")
console.log()
console.log("    #region fixed arrays")
console.log()
for (let n = 1; n <= 6; n++) {
    console.log(`    internal struct Fixed${n}<T> : IFixedArray<T>`)
    console.log(`    {`)
    for (let i = 0; i < n; i++) console.log(`        private T _item${i};`)
    console.log()
    console.log(`        internal Fixed${n}(${commaSeparated(n, i => `T item${i}`)}) =>`)
    console.log(`            (${commaSeparated(n, i => `_item${i}`)}) = (${commaSeparated(n, i => `item${i}`)});`)
    console.log()
    if (n == 1) {
        console.log(`        public static implicit operator Fixed${n}<T>(T value) => new Fixed${n}<T>(value);`)
    } else {
        console.log(`        public static implicit operator Fixed${n}<T>((${commaSeparated(n, _ => `T`)}) tuple) =>`)
        console.log(`            new Fixed${n}<T>(${commaSeparated(n, i => `tuple.Item${i + 1}`)});`)
    }
    console.log()
    console.log(`        public int Length => ${n};`)
    console.log(`        public T this[int i]`)
    console.log(`        {`)
    console.log(`            get => Utils.CheckedIndexing<T>(ref _item0, i, ${n});`)
    console.log(`            set => Utils.CheckedIndexing<T>(ref _item0, i, ${n}) = value;`)
    console.log(`        }`)
    console.log(`    }`)
    console.log()
}
console.log("    #endregion fixed arrays")
console.log("}")
console.log()
console.log("////////////////////////////////////////////////////////////////////////////////////////////////")
console.log("")
// type safe apis
console.log("namespace Anatawa12.AnimatorControllerAsACode.Framework")
console.log("{")

///////// DO NOT FORGET TO FIX EnumWrapper.cs

// typename, prefix, type params
let types: [string, string, string][] = [
    ["int", "Int", "Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>"],
    ["bool", "Bool", "Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>"],
    ["float", "Float", "Fixed1<EditorCurveBinding>, Fixed1<float>, Fixed1<List<Keyframe>>"],
    ["Color", "Color", "Fixed4<EditorCurveBinding>, Fixed4<float>, Fixed4<List<Keyframe>>"],
    ["Vector2", "Vector2", "Fixed2<EditorCurveBinding>, Fixed2<float>, Fixed2<List<Keyframe>>"],
    ["Vector2Int", "Vector2Int", "Fixed2<EditorCurveBinding>, Fixed2<float>, Fixed2<List<Keyframe>>"],
    ["Vector3", "Vector3", "Fixed3<EditorCurveBinding>, Fixed3<float>, Fixed3<List<Keyframe>>"],
    ["Vector3Int", "Vector3Int", "Fixed3<EditorCurveBinding>, Fixed3<float>, Fixed3<List<Keyframe>>"],
    ["Vector4", "Vector4", "Fixed4<EditorCurveBinding>, Fixed4<float>, Fixed4<List<Keyframe>>"],
    ["Quaternion", "Quaternion", "Fixed4<EditorCurveBinding>, Fixed4<float>, Fixed4<List<Keyframe>>"],
    ["Rect", "Rect", "Fixed4<EditorCurveBinding>, Fixed4<float>, Fixed4<List<Keyframe>>"],
    ["RectInt", "RectInt", "Fixed4<EditorCurveBinding>, Fixed4<float>, Fixed4<List<Keyframe>>"],
    ["Bounds", "Bounds", "Fixed6<EditorCurveBinding>, Fixed6<float>, Fixed6<List<Keyframe>>"],
    ["BoundsInt", "BoundsInt", "Fixed6<EditorCurveBinding>, Fixed6<float>, Fixed6<List<Keyframe>>"],
];
console.log("")
console.log("    #region wrapper types")
console.log("")
for (let [type, prefix, args] of types) {
    console.log(`    public class ${prefix}SettingCurve`)
    console.log(`    {`)
    console.log(`        private readonly SettingCurveImpl<${type}, ${prefix}TypeInfo, ${args}> _impl;`)
    console.log()
    console.log(`        internal ${prefix}SettingCurve(AnimationClip clip, EditorCurveBinding binding)`)
    console.log(`        {`)
    console.log(`            _impl = new SettingCurveImpl<${type}, ${prefix}TypeInfo, ${args}>(clip, binding);`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingCurve WithOneFrame(${type} desiredValue)`)
    console.log(`        {`)
    console.log(`            _impl.WithOneFrame(desiredValue);`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingCurve WithFixedSeconds(float seconds, ${type} desiredValue)`)
    console.log(`        {`)
    console.log(`            _impl.WithFixedSeconds(seconds, desiredValue);`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingCurve WithSecondsUnit(Action<${prefix}SettingKeyframes> action)`)
    console.log(`        {`)
    console.log(`            _impl.WithSecondsUnit(impl => action(new ${prefix}SettingKeyframes(impl)));`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingCurve WithFrameCountUnit(Action<${prefix}SettingKeyframes> action)`)
    console.log(`        {`)
    console.log(`            _impl.WithFrameCountUnit(impl => action(new ${prefix}SettingKeyframes(impl)));`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingCurve WithUnit(AccUnit unit, Action<${prefix}SettingKeyframes> action)`)
    console.log(`        {`)
    console.log(`            _impl.WithUnit(unit, impl => action(new ${prefix}SettingKeyframes(impl)));`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`    }`)
    console.log()
    console.log(`    public class ${prefix}SettingKeyframes`)
    console.log(`    {`)
    console.log(`        private readonly SettingKeyframesImpl<${type}, ${prefix}TypeInfo, ${args}> _impl;`)
    console.log()
    console.log(`        internal ${prefix}SettingKeyframes(SettingKeyframesImpl<${type}, ${prefix}TypeInfo, ${args}> impl)`)
    console.log(`        {`)
    console.log(`            _impl = impl;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingKeyframes Easing(float timeInUnit, ${type} value)`)
    console.log(`        {`)
    console.log(`            _impl.Easing(timeInUnit, value);`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingKeyframes Constant(float timeInUnit, ${type} value)`)
    console.log(`        {`)
    console.log(`            _impl.Constant(timeInUnit, value);`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log()
    console.log(`        public ${prefix}SettingKeyframes Linear(float timeInUnit, ${type} value)`)
    console.log(`        {`)
    console.log(`            _impl.Linear(timeInUnit, value);`)
    console.log(`            return this;`)
    console.log(`        }`)
    console.log(`    }`)
    console.log()
    console.log()
}
console.log("")
console.log("    #endregion wrapper types")
console.log("")
console.log("    #region AccEditClip Animates function")
console.log()
console.log(`    public partial struct AccEditClip`)
console.log(`    {`)
for (let [type, prefix, _] of types) {
    console.log(``)
    console.log(`        public ${prefix}SettingCurve Animates${prefix}(string path, Type type, string propertyName)`)
    console.log(`        {`)
    console.log(`            var binding = new EditorCurveBinding`)
    console.log(`            {`)
    console.log(`                path = path,`)
    console.log(`                type = type,`)
    console.log(`                propertyName = propertyName`)
    console.log(`            };`)
    console.log(`            return new ${prefix}SettingCurve(_clip, binding);`)
    console.log(`        }`)
    console.log(``)
    console.log(`        public ${prefix}SettingCurve Animates${prefix}(Transform transform, Type type, string propertyName) =>`)
    console.log(`            Animates${prefix}(_config.ResolveRelativePath(transform), type, propertyName);`)
    console.log(``)
    console.log(`        public ${prefix}SettingCurve Animates${prefix}(Component component, string property) =>`)
    console.log(`            Animates${prefix}(_config.ResolveRelativePath(component.transform), component.GetType(), property);`)
    console.log(``)
    console.log(`        public ${prefix}SettingCurve Animates<TComponent>(TComponent component, Expression<Func<TComponent, ${type}>> property)`)
    console.log(`            where TComponent : Component =>`)
    console.log(`            Animates${prefix}(component, ClipExpressionSupport.CreatePath(component, property));`)
    console.log(``)
}
console.log(`    }`)
console.log()
console.log("    #endregion AccEditClip Animates function")
console.log("}")
