using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected int _currentHealth;
    protected string _uid;
    protected int _level;
    protected List<ResourceValue> _production;
    protected List<SkillManager> _skillManagers;
    protected float _fieldOfView;
    protected int _owner;
    
    public string Uid { get => _uid; }
    public int Level { get => _level; }
    public List<ResourceValue> Production { get => _production; }
    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHealth; set => _currentHealth = value; }
    public int MaxHP { get => _data.healthpoints; }
    public float FieldOfView => _fieldOfView;
    public List<SkillManager> SkillManagers { get => _skillManagers; }
    public int Owner { get => _owner; }


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
        _production = production;
        _fieldOfView = data.fieldOfView;
        _transform.GetComponent<UnitManager>().Initialize(this);
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
                Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);
            }
            
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);
        }
    }

    public bool CanBuy()
    {
        return _data.CanBuy();
    }
    
    public void ProduceResources()
    {
        foreach (ResourceValue resource in _production)
            Globals.GAME_RESOURCES[resource.code].AddAmount(resource.amount);
    }
    
    public void TriggerSkill(int index, GameObject target = null)
    {
        _skillManagers[index].Trigger(target);
    }
    

    
}
