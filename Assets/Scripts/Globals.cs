using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InGameResource
{
    Gold,
    Wood,
    Stone
}
public class Globals
{
    public static int TERRAIN_LAYER_MASK = 1 << 6;
    public static int FLAT_TERRAIN_LAYER_MASK = 1 << 8;
    public static int UNIT_MASK = 1 << 10;
    public static int TREE_MASK = 1 << 11;
    public static int ROCK_MASK = 1 << 12;

    public static BuildingData[] BUILDING_DATA;
    
    public static Dictionary<InGameResource, GameResource> GAME_RESOURCES =
    new Dictionary<InGameResource, GameResource>()
    {
        { InGameResource.Gold, new GameResource("Gold", 300) },
        { InGameResource.Wood, new GameResource("Wood", 300) },
        { InGameResource.Stone, new GameResource("Stone", 300) }
    };
    
    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
}
