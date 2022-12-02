using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class GetPosNearHurtAlly : ActionNode
{

    public float range = 3f;
    
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (!context.SAi.HasTargetCharacter())
        {
            Debug.Log($"{context.gameObject.name} Failed to get pos near hurt ally because it has no target");
            return State.Failure;
        }
        var pt = Random.insideUnitCircle * range;
        var targetPos = context.SAi.targetChar.gameObject.transform.position;
        var randomPos = new Vector3(targetPos.x + pt.x,
            targetPos.y, targetPos.z + pt.y);
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(randomPos, out navMeshHit, 10f, NavMesh.AllAreas))
        {
            blackboard.moveToPosition = navMeshHit.position;
            return State.Success;
        }
        Debug.Log($"{context.gameObject.name} Failed to get pos near hurt ally because NavMesh sample failed. Targetchar pos is {context.SAi.targetChar.gameObject.transform.position}");
        context.SAi.SetTargetCharacter(null);
        return State.Failure;
    }
}
