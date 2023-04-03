using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Parameters", menuName = "Scriptable Objects/Game Global Parameters", order = 10)]
public class GameGlobalParameters : GameParameters
{
    public override string GetParametersName() => "Global";
    
    [Header("Day and Night")]
    public bool enableDayAndNightCycle;
    public float dayLengthInSeconds;
    public float dayInitialRatio;
    
    [Header("Initialization")]
    public BuildingData initialBuilding;
    
    [Header("Production bonuses")]
    public int baseGoldProduction;
    public int bonusGoldProductionPerBuilding;
    public float goldBonusRange;
    public float woodProductionRange;
    public float stoneProductionRange;
    
    [Header("FOV")]
    public bool enableFOV;
    
    public delegate int ResourceProductionFunc(float distance);
    
    [HideInInspector]
    public ResourceProductionFunc woodProductionFunc = (float distance) =>
    {
        return Mathf.CeilToInt(10 * 1f / distance);
    };
    [HideInInspector]
    public ResourceProductionFunc stoneProductionFunc = (float distance) =>
    {
        return Mathf.CeilToInt(2 * 1f / distance);
    };
    
    public int UnitMaxLevel()
    {
        Keyframe[] keys = experienceEvolutionCurve.keys;
        return (int)keys.Select(k => k.time).Max();
    }
    
    public AnimationCurve experienceEvolutionCurve;
    public AnimationCurve productionMultiplierCurve;
    public AnimationCurve attackDamageMultiplierCurve;
    public AnimationCurve attackRangeMultiplierCurve;
    
}
