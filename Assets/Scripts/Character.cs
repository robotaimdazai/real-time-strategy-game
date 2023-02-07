using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Unit
{
    public Character(CharacterData data, int owner) :
        base(data,owner,new List<ResourceValue>() { }) {}
}