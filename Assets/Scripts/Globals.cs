using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals
{
    public static int TERRAIN_LAYER_MASK = 1 << 6;

    public static BuildingData[] BUILDING_DATA;
    
    public static Dictionary<string, GameResource> GAME_RESOURCES =
    new Dictionary<string, GameResource>()
    {
        { "gold", new GameResource("Gold", 300) },
        { "wood", new GameResource("Wood", 300) },
        { "stone", new GameResource("Stone", 300) }
    };
    
    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
}
