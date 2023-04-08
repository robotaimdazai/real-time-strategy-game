using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(GameParameters),true)]
public class GameParametersEditor : Editor
{
    private delegate void DDrawPrefix(Object obj, FieldInfo field);
    public  void OnInspectorGUI_LEGACY()
    {
        GameParameters parameters = (GameParameters)target;
        
        EditorGUILayout.LabelField($"Name: {parameters.GetParametersName()}", EditorStyles.boldLabel);

        var type = parameters.GetType();
        var fields = type.GetFields();

        DDrawPrefix drawPrefix = (Object obj, FieldInfo field) =>
        {
            GameParameters gameParameters = obj as GameParameters;
            var buttonName = gameParameters.ShowsField(field.Name) ? "-" : "+";
            if (GUILayout.Button(buttonName,GUILayout.Width(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(gameParameters);
                AssetDatabase.SaveAssets();
            }
        };
        for (int i = 0; i < fields.Length; i++)
        {
            _DrawField(parameters, fields[i], drawPrefix);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GameParameters parameters = (GameParameters)target;
        EditorGUILayout.LabelField(parameters.GetParametersName(),EditorStyles.boldLabel);
        var type = parameters.GetType();
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            bool isHidden = Attribute.IsDefined(field, typeof(HideInInspector), false);
            if(isHidden)
                continue;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(40f));
            
            //check for header attribute
            bool hasHeader = System.Attribute.IsDefined(field, typeof(HeaderAttribute), false);
            if (hasHeader)
                GUILayout.FlexibleSpace();
                
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(parameters.ShowsField(field.Name) ? "-" : "+", GUILayout.Width(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button(parameters.SerializesField(field.Name) ? "-" : "+", GUILayout.Width(20f)))
            {
                parameters.ToggleSerializeField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name), true);
            EditorGUILayout.EndHorizontal();
        }
        serializedObject.ApplyModifiedProperties();
    }

    //legacy
    void _DrawField(Object obj, FieldInfo field, DDrawPrefix drawPrefix = null)
    {
        if (System.Attribute.IsDefined(field, typeof(HideInInspector), false))
            return;

        EditorGUILayout.BeginHorizontal();
        
        if (drawPrefix != null)
            drawPrefix(obj, field);
        
        EditorGUILayout.LabelField(field.Name);

        if (field.FieldType == typeof(string))
            field.SetValue(
                obj,
                EditorGUILayout.TextField((string)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(bool))
            field.SetValue(
                obj,
                EditorGUILayout.Toggle((bool)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(int))
        {
            if (Attribute.IsDefined(field, typeof(RangeAttribute), false))
            {
                var rangeAttribute = Attribute.GetCustomAttribute(field, typeof(RangeAttribute)) as RangeAttribute;
                field.SetValue(obj,EditorGUILayout.IntSlider(
                    (int)field.GetValue(obj),
                    (int)rangeAttribute.min,
                    (int)rangeAttribute.max));
            }
            else
            {
                field.SetValue(
                    obj,
                    EditorGUILayout.IntField((int)field.GetValue(obj))
                );
            }
            
        }
        else if (field.FieldType == typeof(float))
        {
            if (Attribute.IsDefined(field, typeof(RangeAttribute), false))
            {
                var rangeAttribute = Attribute.GetCustomAttribute(field, typeof(RangeAttribute)) as RangeAttribute;
                field.SetValue(obj,EditorGUILayout.Slider(
                    (float)field.GetValue(obj),
                    rangeAttribute.min,
                    rangeAttribute.max));
            }
            else
            {
                field.SetValue(
                    obj,
                    EditorGUILayout.FloatField((float)field.GetValue(obj))
                );
            }
        }
        else if (field.FieldType == typeof(double))
            field.SetValue(
                obj,
                EditorGUILayout.DoubleField((double)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(long))
            field.SetValue(
                obj,
                EditorGUILayout.LongField((long)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(Rect))
            field.SetValue(
                obj,
                EditorGUILayout.RectField((Rect)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(Vector2))
            field.SetValue(
                obj,
                EditorGUILayout.Vector2Field("", (Vector2)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(Vector3))
            field.SetValue(
                obj,
                EditorGUILayout.Vector3Field("", (Vector3)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(Vector4))
            field.SetValue(
                obj,
                EditorGUILayout.Vector4Field("", (Vector4)field.GetValue(obj))
            );
        else if (field.FieldType == typeof(Color))
            field.SetValue(
                obj,
                EditorGUILayout.ColorField("", (Color)field.GetValue(obj))
            );
        else
            field.SetValue(
                obj,
                EditorGUILayout.ObjectField((Object)field.GetValue(obj), field.FieldType, true)
            );
        EditorGUILayout.EndHorizontal();
    }
}
