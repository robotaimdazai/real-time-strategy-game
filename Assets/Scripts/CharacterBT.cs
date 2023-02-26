using System.Collections;
using System.Collections.Generic;
using BTree;
using UnityEngine;
using Tree = BTree.Tree;

[UnityEngine.RequireComponent(typeof(CharacterManager))]
public class CharacterBT : Tree
{
    CharacterManager manager;
    private void Awake()
    {
        manager = GetComponent<CharacterManager>();
    }
    protected override Node SetupTree()
    {
        Node _root;

        _root = new Parallel(new List<Node> {
            new Sequence(new List<Node> {
                new CheckUnitIsMine(manager),
                new TaskTrySetDestination(manager),
            }),
            new Sequence(new List<Node> {
                new CheckHasDestination(),
                new TaskMoveToDestination(manager),
            })
        });

        return _root;
    }
}
