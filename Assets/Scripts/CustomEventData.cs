using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomEventData
{
    public UnitData unitData;
    public Unit unit;

    public CustomEventData(UnitData unitData)
    {
        this.unitData = unitData;
    }
    
    public CustomEventData(Unit unit)
    {
        this.unitData = null;
        this.unit = unit;
    }
}

public class CustomEvent : UnityEvent<CustomEventData>
{
    
}