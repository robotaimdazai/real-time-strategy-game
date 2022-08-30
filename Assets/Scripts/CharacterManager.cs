using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : UnitManager
{

    private Character _character;
    public override Unit Unit 
    {
        get
        {
            return _character;
        }
        set
        {
            _character = value is Character ? (Character)value : null;
        }
    }
}
