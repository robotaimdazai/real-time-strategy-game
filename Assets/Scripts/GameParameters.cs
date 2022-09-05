using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Parameters", menuName = "Scriptable Objects/Game Parameters", order = 10)]
public class GameParameters : ScriptableObject
{
    public bool enableDayAndNightCycle;
    public float dayLengthInSeconds;
    public float dayInitialRatio;
}
