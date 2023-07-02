using UnityEditor;
using UnityEngine;

namespace Cr7Sund
{
    public static class ScriptableObjectHelper
    {
        public static T CreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            var soItem = ScriptableObject.CreateInstance<T>();
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)))
            {
                Log.Error($"Already exists the same name /n {assetPath}", AssetDatabase.LoadAssetAtPath<T>(assetPath));
            }
            AssetDatabase.CreateAsset(soItem, assetPath);
            return soItem;
        }

    }
}