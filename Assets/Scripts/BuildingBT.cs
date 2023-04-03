using System;
using System.Collections;
using System.Collections.Generic;
using BTree;
using UnityEngine;
using Tree = BTree.Tree;

[UnityEngine.RequireComponent(typeof(BuildingManager))]
public class BuildingBT : Tree
{
    BuildingManager manager;
    private void Awake()
    {
        manager = GetComponent<BuildingManager>();
    }

    protected override Node SetupTree()
    {
        Node _root;

        _root = new Parallel();
        if (manager.Unit.Data.attackDamage > 0)
        {
            var attackSequence = new Sequence(new List<Node>()
            {
                new CheckEnemyInAttackRange(manager),
                new Timer(
                    manager.Unit.Data.attackRate,
                    new List<Node>()
                    {
                        new TaskAttack(manager),
                    }
                    )
            });
            
            _root.Attach(attackSequence);
            _root.Attach(new CheckEnemyInFOVRange(manager));
        }

        _root.Attach( new Sequence(new List<Node>()
        {
            new CheckUnitIsMine(manager),
            new Timer(GameManager.instance.producingRate,
                new List<Node>()
                {
                    new TaskProduceResources(manager)
                },
                delegate
                {
                    EventManager.TriggerEvent("UpdateResourceTexts");
                })
        }));

        return _root;
    }
}
