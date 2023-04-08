using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct UnitLevelUpData
{
    public List<ResourceValue> cost;
    public Dictionary<InGameResource, int> newProduction;
    public int newAttackDamage;
    public float newAttackRange;

    public UnitLevelUpData(
        List<ResourceValue> cost,
        Dictionary<InGameResource, int> newProduction,
        int newAttackDamage,
        float newAttackRange
    )
    {
        this.cost = cost;
        this.newProduction = newProduction;
        this.newAttackDamage = newAttackDamage;
        this.newAttackRange = newAttackRange;
    }
}
public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected int _currentHealth;
    protected string _uid;
    protected int _level;
    protected Dictionary<InGameResource, int> _production;
    protected List<SkillManager> _skillManagers;
    protected float _fieldOfView;
    protected int _owner;
    
    protected int _attackDamage;
    protected float _attackRange;
    protected bool _levelMaxedOut;
    protected UnitLevelUpData _levelUpData;
    
    public string Uid { get => _uid; }
    public int Level { get => _level; }
    public Dictionary<InGameResource, int> Production { get => _production; }
    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHealth; set => _currentHealth = value; }
    public int MaxHP { get => _data.healthpoints; }
    public float FieldOfView => _fieldOfView;
    public List<SkillManager> SkillManagers { get => _skillManagers; }
    public int Owner { get => _owner; }
    public int AttackDamage { get => _attackDamage; }
    public float AttackRange { get => _attackRange; }
    public bool LevelMaxedOut { get => _levelMaxedOut; }
    public UnitLevelUpData LevelUpData { get => _levelUpData; }
    


    public Unit(UnitData data, int owner) : this(data, owner,new List<ResourceValue>() { })
    {
        
    }
    public Unit(UnitData data, int owner, List<ResourceValue> production)
    {
        _data = data;
        _currentHealth = data.healthpoints;
        _owner = owner;

        GameObject g = GameObject.Instantiate(data.prefab) as GameObject;
        _transform = g.transform;
        _transform.GetComponent<UnitManager>().SetOwnerMaterial(owner);

        _skillManagers = new List<SkillManager>();
        SkillManager sm;
        foreach (SkillData skill in _data.skills)
        {
            sm = g.AddComponent<SkillManager>();
            sm.Initialize(skill, g);
            _skillManagers.Add(sm);
        }
        _uid = System.Guid.NewGuid().ToString();
        _level = 1;
        _production = production.ToDictionary(x => x.code,x=>x.amount );
        _fieldOfView = data.fieldOfView;
        _transform.GetComponent<UnitManager>().Initialize(this);
        _attackDamage = data.attackDamage;
        _attackRange = data.attackRange;
        _levelMaxedOut = false;
        _levelUpData = _GetLevelUpData();
    }
    
    public void LevelUp()
    {
        if (_levelMaxedOut)
            return;
        
        _level += 1;
        GameGlobalParameters p = GameManager.instance.gameGlobalParameters;
        // update production
        float prodMultiplier = p.productionMultiplierCurve.Evaluate(_level);
        List<InGameResource> prodResources = _production.Keys.ToList();
        foreach (InGameResource r in prodResources)
            _production[r] = (int)((_production[r] + 0.1f) * prodMultiplier);
        
        // update attack
        float attDamageMultiplier = p.attackDamageMultiplierCurve.Evaluate(_level);
        float attRangeMultiplier = p.attackRangeMultiplierCurve.Evaluate(_level);
        _attackDamage = Mathf.CeilToInt(_attackDamage * attDamageMultiplier);
        _attackRange *= attRangeMultiplier;
            
        // consume resources
        
        foreach (ResourceValue resource in _levelUpData.cost)
        {
            Globals.GAME_RESOURCES[_owner][resource.code].AddAmount(-resource.amount);
        }
        EventManager.TriggerEvent("UpdateResourceTexts");
        
        // play sound / show nice VFX
        _transform.GetComponent<UnitManager>().LevelUp();
        _levelMaxedOut = _level == p.UnitMaxLevel();
        _levelUpData = _GetLevelUpData();
        Debug.Log($"Level up to level {_level}!");
    }
    
    public List<ResourceValue> GetLevelUpCost()
    {
        int xpCost = (int)GameManager.instance.gameGlobalParameters.experienceEvolutionCurve.Evaluate(_level + 1);
        return Globals.ConvertXPCostToGameResources(xpCost, Data.cost.Select(v => v.code));
    }
    private UnitLevelUpData _GetLevelUpData()
    {
        GameGlobalParameters p = GameManager.instance.gameGlobalParameters;

        // get updated production
        float prodMultiplier = p.productionMultiplierCurve.Evaluate(_level + 1);
        List<InGameResource> prodResources = _production.Keys.ToList();
        Dictionary<InGameResource, int> newProduction = new Dictionary<InGameResource, int>(_production.Count);
        foreach (InGameResource r in prodResources)
            newProduction[r] = Mathf.CeilToInt(_production[r] * prodMultiplier);

        // get updated attack parameters
        float attDamageMultiplier = p.attackDamageMultiplierCurve.Evaluate(_level + 1);
        int newAttackDamage = Mathf.CeilToInt(_attackDamage * attDamageMultiplier);
        float attRangeMultiplier = p.attackRangeMultiplierCurve.Evaluate(_level + 1);
        float newAttackRange = _attackRange * attRangeMultiplier;

        return new UnitLevelUpData(
            GetLevelUpCost(), newProduction, newAttackDamage, newAttackRange
        );
    }

    public Dictionary<InGameResource, int> ComputeProduction()
    {
        if (_data.canProduce.Length == 0) return null;

        GameGlobalParameters globalParams = GameManager.instance.gameGlobalParameters;
        GamePlayersParameters playerParams = GameManager.instance.gamePlayersParameters;
        Vector3 pos = _transform.position;

        if (_data.canProduce.Contains(InGameResource.Gold))
        {
            int bonusBuildingsCount = Physics.OverlapSphere(pos, globalParams.goldBonusRange, Globals.UNIT_MASK).Where(
                collider =>
                {
                    var buildingManager = collider.GetComponent<BuildingManager>();
                    if (buildingManager == null) return false;
                    return buildingManager.Unit.Owner == playerParams.myPlayerId;
                }).Count();
            
            _production[InGameResource.Gold] = 
                globalParams.baseGoldProduction + bonusBuildingsCount * globalParams.bonusGoldProductionPerBuilding;
        }

        if (_data.canProduce.Contains(InGameResource.Wood))
        {
            int treeScore = Physics.OverlapSphere(pos, globalParams.woodProductionRange, Globals.TREE_MASK).Select(
                collider =>
                {
                    var distance = Vector3.Distance(pos, collider.transform.position);
                    var score =globalParams.woodProductionFunc(distance);
                    return score;
                }).Sum();
            _production[InGameResource.Wood] = treeScore;
        }
        if (_data.canProduce.Contains(InGameResource.Stone))
        {
            int rocksScore =
                Physics.OverlapSphere(pos, globalParams.woodProductionRange, Globals.ROCK_MASK)
                    .Select((c) => globalParams.stoneProductionFunc(Vector3.Distance(pos, c.transform.position)))
                    .Sum();
            _production[InGameResource.Stone] = rocksScore;
        }

        _levelUpData = _GetLevelUpData();
        return _production;
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public virtual void Place()
    {
        _transform.GetComponent<BoxCollider>().isTrigger = false;
        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerId)
        {
            foreach (ResourceValue resource in _data.cost)
            {
                Globals.GAME_RESOURCES[_owner][resource.code].AddAmount(-resource.amount);
            }
            
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);
            
        }
    }

    public bool CanBuy()
    {
        return _data.CanBuy(_owner);
    }
    
    public void ProduceResources()
    {
        foreach (KeyValuePair<InGameResource, int> resource in _production)
        {
            Globals.GAME_RESOURCES[_owner][resource.Key].AddAmount(resource.Value);
        }
    }
    
    public void TriggerSkill(int index, GameObject target = null)
    {
        _skillManagers[index].Trigger(target);
    }
    

    
}
