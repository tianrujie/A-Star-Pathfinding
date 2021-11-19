using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DamageTextAnimationSetting))]
public class DamageTextAnimationSettingInspector : Editor
{
    private bool isExpanded = true;
    private ReorderableList m_Settings;

    private void OnEnable()
    {
        SerializedProperty prop = serializedObject.FindProperty("settings");
        DamageTextAnimationSetting comp = target as DamageTextAnimationSetting;

        m_Settings = new ReorderableList(serializedObject, prop, true, true, true, true);

        m_Settings.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = prop.GetArrayElementAtIndex(index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.PropertyField(rect, element);
        };

        m_Settings.drawHeaderCallback = (rect) =>
        {
            EditorGUI.LabelField(rect, prop.displayName);
        };

        m_Settings.onSelectCallback = (ReorderableList list) =>
        {
            GameObject go = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("setting").objectReferenceValue as GameObject;
            if (go != null)
            {
                EditorGUIUtility.PingObject(go.gameObject);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        serializedObject.Update();

        isExpanded = EditorGUILayout.Foldout(isExpanded, "Animation Settings", true);
        if (isExpanded)
        {
            EditorGUI.indentLevel++;
            m_Settings.DoLayoutList();
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}


[CustomPropertyDrawer(typeof(DamageTextAnimationItem))]
public class DamageTextAnimationItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            EditorGUIUtility.labelWidth = 50;
            position.height = EditorGUIUtility.singleLineHeight;

            Rect keyRect = new Rect(position)
            {
                x = position.x - 10,
                width = 110,
            };

            Rect goRect = new Rect(keyRect)
            {
                x = keyRect.x + keyRect.width,
                width = position.width - keyRect.width
            };

            SerializedProperty keyProperty = property.FindPropertyRelative("key");
            SerializedProperty goProperty = property.FindPropertyRelative("setting");

            if (keyProperty.stringValue == "")
            {
                GUI.backgroundColor = Color.red;
            }
            else
            {
                GUI.backgroundColor = Color.green;
            }
            keyProperty.stringValue = EditorGUI.TextField(keyRect, keyProperty.displayName, keyProperty.stringValue);

            if (goProperty.objectReferenceValue as GameObject == null)
            {
                GUI.backgroundColor = Color.red;
            }
            else
            {
                GUI.backgroundColor = Color.green;
            }
            EditorGUI.PropertyField(goRect, goProperty, GUIContent.none);
        }
    }
}
