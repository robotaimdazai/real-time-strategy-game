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
    
    public string Uid { get => _uid; }
    public int Level { get => _level; }
    public List<ResourceValue> Production { get => _production; }
    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHealth; set => _currentHealth = value; }
    public int MaxHP { get => _data.healthpoints; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }


    public Unit(UnitData data) : this(data, new List<ResourceValue>() { })
    {
    }
    public Unit(UnitData data, List<ResourceValue> production)
    {
        _data = data;
        _currentHealth = data.healthpoints;

        GameObject g = GameObject.Instantiate(data.prefab) as GameObject;
        _transform = g.transform;
        
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
        _transform.Find("fov").transform.localScale = new Vector3(data.fieldOfView, data.fieldOfView, 0f);
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public virtual void Place()
    {
        
        _transform.GetComponent<BoxCollider>().isTrigger = false;
        foreach (ResourceValue resource in _data.cost)
        {
            Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);
        }
        _transform.GetComponent<UnitManager>().EnableFOV();
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
