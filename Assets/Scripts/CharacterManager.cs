using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : UnitManager
{

    public UnityEngine.AI.NavMeshAgent agent;
    private Character _character;
    public override Unit Unit 
    {
        get
        {
            return _character;
        }
        set
        {
            _character = value is Character ? (Character)value : null;
        }
    }
    
    public bool MoveTo(Vector3 targetPosition)
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        if (path.status == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            contextualSource.PlayOneShot(((CharacterData)Unit.Data).onMoveInvalidSound);
            return false;
        }

        agent.destination = targetPosition;
        contextualSource.PlayOneShot(((CharacterData)Unit.Data).onMoveValidSound);
        return true;
    }
}
