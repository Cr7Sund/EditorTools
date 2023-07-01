namespace Cr7Sund
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CreateAssetMenu(fileName = "New menu", menuName = "Data/New Item", order = 1)]
    public class BaseMenuItem : ScriptableObject
    {
        [HideInEditor]
        public string menuName;
        [HideInEditor]
        public string description;
        [HideInEditor]
        public VisualTreeAsset uxmlFile;
        public Categories category;
        [Label("Reanem Stackable")]
        public bool stackable;
        public int buyprice;
        [Range(0, 1)]
        public float sellpercentage;

        [HideInEditor]
        public Sprite icon;

        public enum Categories
        {
            Armor,
            Food,
            Potion,
            Weapon,
            Junk
        }

        [HideInEditor]

        public List<BaseMenuItem> childMenuItems = new List<BaseMenuItem>();


    }
}