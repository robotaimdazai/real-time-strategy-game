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
    private TaskTrySetDestinationOrTarget _trySetDestinationOrTargetNode;
    private void Awake()
    {
        manager = GetComponent<CharacterManager>();
    }
    
    private void OnEnable()
    {
        EventManager.AddListener("TargetFormationOffsets", _OnTargetFormationOffsets);
        EventManager.AddListener("TargetFormationPositions", _OnTargetFormationPositions);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("TargetFormationOffsets", _OnTargetFormationOffsets);
        EventManager.RemoveListener("TargetFormationPositions", _OnTargetFormationPositions);
    }

    private void _OnTargetFormationOffsets(object data)
    {
        List<UnityEngine.Vector2> targetOffsets = (List<UnityEngine.Vector2>)data;
        _trySetDestinationOrTargetNode.SetFormationTargetOffset(targetOffsets);
    }

    private void _OnTargetFormationPositions(object data)
    {
        List<UnityEngine.Vector3> targetPositions = (List<UnityEngine.Vector3>)data;
        _trySetDestinationOrTargetNode.SetFormationTargetPosition(targetPositions);
    }
    protected override Node SetupTree()
    {
        Node _root;
        
        //subtree
        _trySetDestinationOrTargetNode = new TaskTrySetDestinationOrTarget(manager);
        Sequence trySetDestinationOrTargetSequence = new Sequence(new List<Node> {
            new CheckUnitIsMine(manager),
            _trySetDestinationOrTargetNode,
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
