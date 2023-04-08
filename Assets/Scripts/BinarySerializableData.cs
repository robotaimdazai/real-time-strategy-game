using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class BinarySerializableData 
{
    private static List<Type> _serializableTypes = new List<Type>()
    {
        typeof(int),
        typeof(float),
        typeof(bool),
        typeof(string),
        typeof(InputBinding),
        typeof(PlayerData)
    };
    
    
    public Dictionary<string, object> properties;
    
    public BinarySerializableData(ScriptableObject obj, List<string> fieldsToSerialize)
    {
        properties = new Dictionary<string, object>();

        Type T = obj.GetType();
        foreach (FieldInfo field in T.GetFields())
        {
            if (!fieldsToSerialize.Contains(field.Name))
                continue;

            object value;
            if (Serialize(field, obj, out value))
                properties[field.Name] = value;
        }
    }
    
    private static bool _IsTypeSerializable(Type tested)
    {
        //if the type exists in the list of serializable or it is array of serializable 
        return
            _serializableTypes.Contains(tested) ||
            tested.IsArray && _serializableTypes.Contains(tested.GetElementType());
    }

    private static bool _IsOfType(Type tested, Type reference)
    {
        return
            tested == reference ||
            tested.IsArray && tested.GetElementType() == reference;
    }
    
    public static bool Serialize(FieldInfo field, object obj, out object value)
    {
        return _SerializeValue(field.FieldType, field.GetValue(obj), out value);
    }

    public static bool Deserialize(FieldInfo field, object data, out object value)
    {
        Type T = field.FieldType;
        if (_IsTypeSerializable(T))
        {
            value = data;
            return true;
        }
        else if (_IsOfType(T, typeof(Color)))
        {
            float[] c = (float[]) data;
            value = new Color(c[0], c[1], c[2], c[3]);
            return true;
        }
        
        value = null;
        return false;
        
    }
    
    public static Type GetSerializedType(FieldInfo field)
    {
        Type T = field.FieldType;
        if (_IsTypeSerializable(T))
            return T;

        object serialized;
        _SerializeValue(T, T.IsValueType ? Activator.CreateInstance(T) : null, out serialized);
        return serialized.GetType();
    }
    private static bool _SerializeValue(Type T, object inValue, out object outValue)
    {
        if (_IsTypeSerializable(T))
        {
            outValue = inValue;
            return true;
        }
        else if (_IsOfType(T, typeof(Color)))
        {
            Color c = (Color)inValue;
            outValue = new float[] { c.r, c.g, c.b, c.a };
            return true;
        }

        outValue = null;
        return false;
    }
    

}
