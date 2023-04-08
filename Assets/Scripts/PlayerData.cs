using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class PlayerData:BinarySerializable
{
    public string name;
    public Color color;
    
    protected PlayerData(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
