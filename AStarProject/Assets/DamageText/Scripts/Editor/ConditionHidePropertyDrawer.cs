using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ConditionHideAttribute))]
public class ConditionHidePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionHideAttribute conditionHide = (ConditionHideAttribute)attribute;
        bool show = property.serializedObject.FindProperty(conditionHide.condition).boolValue;
        bool oldShow = GUI.enabled;
        GUI.enabled = show;
        if (show)
        {
            EditorGUI.Slider(position, property, conditionHide.rangeMin, conditionHide.rangeMax);
        }        
        GUI.enabled = oldShow;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionHideAttribute conditionHide = (ConditionHideAttribute)attribute;
        bool show = property.serializedObject.FindProperty(conditionHide.condition).boolValue;
        if (show)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
