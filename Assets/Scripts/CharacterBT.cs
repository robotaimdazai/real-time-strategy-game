using System.Collections;
using System.Collections.Generic;
using BTree;
using UnityEngine;
using Tree = BTree.Tree;

[UnityEngine.RequireComponent(typeof(CharacterManager))]
public class CharacterBT : Tree
{
    CharacterManager manager;
    
    private Ray _ray;
    private RaycastHit _raycastHit;
    private void Awake()
    {
        manager = GetComponent<CharacterManager>();
    }
    protected override Node SetupTree()
    {
        Node _root;
        
        //subtree
        Sequence trySetDestinationOrTargetSequence = new Sequence(new List<Node> {
            new CheckUnitIsMine(manager),
            new TaskTrySetDestinationOrTarget(manager),
        });
        //subtree
        Sequence moveToDestinationSequence = new Sequence(new List<Node> {
            new CheckHasDestination(),
            new TaskMoveToDestination(manager),
        });
        //subtree
        Sequence attackSequence = new Sequence(new List<Node> {
            new Inverter(new List<Node>
            {
                new CheckTargetIsMine(manager),
            }),
            new CheckEnemyInAttackRange(manager),
            new Timer(
                manager.Unit.Data.attackRate,
                new List<Node>()
                {
                    new TaskAttack(manager)
                }
            ),
        });
        //subtree
        Sequence moveToTargetSequence = new Sequence(new List<Node> {
            new CheckHasTarget()
        });
        
        //adding child on check basis
        if (manager.Unit.Data.attackDamage > 0)
        {
            moveToTargetSequence.Attach(new Selector(new List<Node> {
                attackSequence,
                new TaskFollow(manager),
            }));
        }
        else
        {
            moveToTargetSequence.Attach(new TaskFollow(manager));
        }

        //select among 2 subtrees,
        //subtree 1: parallel, runs both child
        //if both child subtrees of subtree 1 fail then move to  subtree2
        //subtree 2: just check if enemy is in range, set target if so
        _root = new Selector(new List<Node> {
            new Parallel(new List<Node> {
                trySetDestinationOrTargetSequence,
                new Selector(new List<Node>
                {
                    moveToDestinationSequence,
                    moveToTargetSequence,
                }),
            }),
            new CheckEnemyInFOVRange(manager),
        });

        return _root;
    }
}
