using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Unit
{
    public Character(CharacterData data) :
        base(data, new List<ResourceValue>() { }) {}
}