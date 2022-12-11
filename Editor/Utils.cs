using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Anatawa12.AnimatorControllerAsACode.Editor
{
    internal static class Utils
    {
        // .NET 4.x which is used in unity doesn't have Deconstruct method so I declare here
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value)
        {
            key = self.Key;
            value = self.Value;
        }

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

    }
}
