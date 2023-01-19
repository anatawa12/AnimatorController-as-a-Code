using System;
using System.Collections.Generic;
using Anatawa12.AnimatorControllerAsACode.Framework;
using JetBrains.Annotations;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    internal static class Utils
    {
        // Used in Linq
        public static T GetOrKey<T>(this IDictionary<T, T> self, T key) =>
            self.TryGetValue(key, out var value) ? value : key;
        
        [CanBeNull]
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key) =>
            self.TryGetValue(key, out var found) ? found : default;

        // ReSharper disable InconsistentNaming
        public static T LoadAssetAtGUID<T>(string guid) where T : Object =>
            AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        public static T LoadAssetAtGUID<T>(GUID guid) where T : Object => LoadAssetAtGUID<T>(guid.ToString());

        public static GUID AssetPathToGUID(string path) =>
            GUID.TryParse(AssetDatabase.AssetPathToGUID(path), out var guid)
                ? guid
                : throw new ArgumentException("GUID for that path not found", nameof(path));

        public static GUID GetAssetGUID(Object generator)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(generator, out var guidString, out long _))
                throw new InvalidOperationException("GUID for asset not found");
            if (!GUID.TryParse(guidString, out var guid))
                throw new InvalidOperationException("logic failure");
            return guid;
        }
        // ReSharper restore InconsistentNaming

        // to access error obsolete property. it's error obsolete because disallow accessing without those methods.
        // we cannot disable CS0019 so replace it with CS0018 using those warning obsolete methods
        [Obsolete]
        private static AnimatorControllerGenerator GetGenerator1(GeneratorLayerBase layer) =>
            layer.generator as AnimatorControllerGenerator;
        [Obsolete]
        private static void SetGenerator1(GeneratorLayerBase layer, AnimatorControllerGenerator generator) =>
            layer.generator = generator;

#pragma warning disable CS0612
        public static AnimatorControllerGenerator GetGenerator(this GeneratorLayerBase layer) =>
            GetGenerator1(layer);
        public static void SetGenerator(this GeneratorLayerBase layer, AnimatorControllerGenerator generator) =>
            SetGenerator1(layer, generator);
#pragma warning restore CS0612
    }
}
