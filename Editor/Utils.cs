using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
    }
}
