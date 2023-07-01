using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cr7Sund
{
    public class VisualElementHelper
    {
        public static BindableElement GetMatchBindableElement(SerializedProperty serializeProperty, Dictionary<Type, Attribute> attributeDict)
        {
            var propertyType = serializeProperty.propertyType;

            string labelName = serializeProperty.displayName;
            if (attributeDict.TryGetValue(typeof(ButtonAttribute), out var btnAttribute) &&
            btnAttribute is ButtonAttribute btnAttr)
            {
                labelName = btnAttr.labelName;
            }

            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    return new IntegerField(labelName);
                case SerializedPropertyType.Boolean:
                    return new Toggle(labelName);
                case SerializedPropertyType.Float:
                    if (attributeDict.ContainsKey(typeof(RangeAttribute)))
                    {
                        return new Slider(labelName);
                    }
                    return new FloatField(labelName);
                case SerializedPropertyType.String:
                    return new TextField(labelName);
                case SerializedPropertyType.Color:
                    return new ColorField(labelName);
                case SerializedPropertyType.ObjectReference:
                    return new ObjectField(labelName);
                case SerializedPropertyType.LayerMask:
                    return new LayerMaskField(labelName);
                case SerializedPropertyType.Enum:
                    return new EnumField(labelName);
                case SerializedPropertyType.Vector2:
                    return new Vector2Field(labelName);
                case SerializedPropertyType.Vector3:
                    return new Vector3Field(labelName);
                case SerializedPropertyType.Vector4:
                    return new Vector4Field(labelName);
                case SerializedPropertyType.Rect:
                    return new RectField(labelName);
                case SerializedPropertyType.AnimationCurve:
                    return new CurveField(labelName);
                case SerializedPropertyType.Bounds:
                    return new BoundsField(labelName);
                case SerializedPropertyType.Gradient:
                    return new GradientField(labelName);
                default:
                    Log.Error($"{propertyType} is not supported");
                    return null;
            }
        }

    }
}