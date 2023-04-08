using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Unit", order = 1)]

public class UnitData : ScriptableObject
{
        public string code;
        public string unitName;
        public string description;
        public int healthpoints;
        public float fieldOfView;
        public GameObject prefab;
        public List<ResourceValue> cost;
        public List<SkillData> skills = new List<SkillData>();
        [Header("General Sounds")]
        public AudioClip onSelectSound;
        public InGameResource[] canProduce;
        public float attackRange;
        public int attackDamage;
        public float attackRate;
        
        
        public bool CanBuy(int owner)
        {
            return Globals.CanBuy(owner, cost);
        }
    
        
}
