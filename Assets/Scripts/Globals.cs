using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    public static Dictionary<InGameResource, GameResource>[] GAME_RESOURCES;
    public static void InitializeGameResources(int nPlayers)
    {
        GAME_RESOURCES = new Dictionary<InGameResource, GameResource>[nPlayers];
        for (int i = 0; i < nPlayers; i++)
            GAME_RESOURCES[i] = new Dictionary<InGameResource, GameResource>()
            {
                { InGameResource.Gold, new GameResource("Gold", 1000) },
                { InGameResource.Wood, new GameResource("Wood", 1000) },
                { InGameResource.Stone, new GameResource("Stone", 1000) }
            };
    }
    
    public static Dictionary<InGameResource, int> XP_CONVERSION_TO_RESOURCE = new Dictionary<InGameResource, int>()
    {
        { InGameResource.Gold, 100 },
        { InGameResource.Wood, 80 },
        { InGameResource.Stone, 40 }
    };
    
    public static List<ResourceValue> ConvertXPCostToGameResources(int xpCost, IEnumerable<InGameResource> allowedResources)
    {
        // distribute the xp cost between all possible resources, always
        // starting with 1 unit of every allowed resource type and then
        // picking the rest from allowed resource types

        // sort resources by xp cost
        List<InGameResource> sortedResources = allowedResources
            .OrderBy(r => XP_CONVERSION_TO_RESOURCE[r])
            .ToList();
        int n = sortedResources.Count();


        Dictionary<InGameResource, int> xpCostToResources = new Dictionary<InGameResource, int>();
        foreach (InGameResource r in sortedResources)
        {
            if (xpCost == 0) break;
            xpCostToResources[r] = 1;
            xpCost--;
        }

        int i = 0;
        while (xpCost > 0)
        {
            xpCostToResources[sortedResources[i]]++;
            xpCost--;
            i = (i + 1) % n;
        }

        return xpCostToResources
            .Select(pair => new ResourceValue(pair.Key, pair.Value * XP_CONVERSION_TO_RESOURCE[pair.Key]))
            .ToList();
    }
    
    public static bool CanBuy(List<ResourceValue> cost)
    {
        return CanBuy(GameManager.instance.gamePlayersParameters.myPlayerId, cost);
    }
    public static bool CanBuy(int playerId, List<ResourceValue> cost)
    {
        foreach (ResourceValue resource in cost)
            if (GAME_RESOURCES[playerId][resource.code].Amount < resource.amount)
                return false;
        return true;
    }
    
    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();
}
