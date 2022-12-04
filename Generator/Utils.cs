using UnityEditor;
using UnityEngine;

namespace Anatawa12.AnimatorControllerAsACode.Generator
{
    internal static class Utils
    {
        public static void AddToFile(Object file, Object obj)
        {
            AssetDatabase.AddObjectToAsset(obj, file);
        }
    }
}