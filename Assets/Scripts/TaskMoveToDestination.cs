using System.Collections;
using System.Collections.Generic;
using BTree;
using UnityEngine;

public class TaskMoveToDestination : Node
{
    CharacterManager _manager;
    
    public TaskMoveToDestination(CharacterManager manager) : base()
    {
        _manager = manager;
    }
    
    public override NodeState Evaluate()
    {
        object destinationPoint = GetData("destinationPoint");
        Vector3 destination = (Vector3) destinationPoint;
        
        if (Vector3.Distance(destination, _manager.agent.destination) > 0.2f)
        {
            bool canMove = _manager.MoveTo(destination);
            _state = canMove ? NodeState.RUNNING : NodeState.FAILURE;
            return _state;
        }
        
        float d = Vector3.Distance(_manager.transform.position, _manager.agent.destination);
        if (d <= _manager.agent.stoppingDistance)
        {
            ClearData("destinationPoint");
            _state = NodeState.SUCCESS;
            return _state;
        }

        _state = NodeState.RUNNING;
        return _state;
    }
}
