using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public static class DataHandler
{
    public static void LoadGameData()
    {
        Globals.BUILDING_DATA = Resources.LoadAll<BuildingData>("ScriptableObjects/Units/Buildings");
        
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
            parameters.LoadFromFile();
    }

    public static void SaveGameData()
    {
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
            parameters.SaveToFile();
    }

    
}

