namespace Cr7Sund
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CreateAssetMenu(fileName = "Start menu", menuName = "Data/Start Item", order = 2)]
    public class StartMenuItem : BaseMenuItem
    {
        public int test;

        [Button("Test New")]
        public void Test()
        {
            Log.Debug("Test Start");
        }
    }
}