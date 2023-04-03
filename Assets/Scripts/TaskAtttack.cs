using System.Collections;
using System.Collections.Generic;
using BTree;
using UnityEngine;

public class TaskAttack : Node
{
   
    UnitManager _manager;

    public TaskAttack(UnitManager manager) : base()
    {
        _manager = manager;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = GetData("currentTarget");
        _manager.Attack((Transform) currentTarget);
        _state = NodeState.SUCCESS;
        return _state;
    }
}
