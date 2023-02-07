using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PlayerData))]
public class PlayerDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        label.text = label.text.Replace("Element", "Player");
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        float fullWidth = EditorGUIUtility.labelWidth;
        float nameWidth = fullWidth * 0.7f;
        float colorWidth = fullWidth * 0.3f;

        var nameRect = new Rect(position.x, position.y, nameWidth, position.height);
        Rect colorRect = new Rect(position.x + nameWidth + 5, position.y, colorWidth, position.height);
        
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("color"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

