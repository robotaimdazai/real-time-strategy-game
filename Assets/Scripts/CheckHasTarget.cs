using System.Collections;
using System.Collections.Generic;
using BTree;
using UnityEngine;

public class CheckHasTarget : Node
{
    public override NodeState Evaluate()
    {
        object currentTarget = Parent.GetData("currentTarget");
        if (currentTarget == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }
        
        object currentTargetOffset = Parent.GetData("currentTargetOffset");
        if (currentTargetOffset == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }
        if (!((Transform) currentTarget))
        {
            Parent.ClearData("currentTarget");
            _state = NodeState.FAILURE;
            return _state;
        }

        _state = NodeState.SUCCESS;
        return _state;
    }
}
