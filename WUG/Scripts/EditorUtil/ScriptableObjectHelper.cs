using UnityEditor;
using UnityEngine;

namespace Cr7Sund
{
    public static class ScriptableObjectHelper
    {
        public static T CreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            var soItem = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(soItem, assetPath);
 
            return soItem;
        }

    }
}