using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class MoveToPosition : ActionNode
{
    public float stoppingDistance = 0.1f;
    public SAi.LookAtBehavior lookAtBehavior = SAi.LookAtBehavior.AtPath;

    protected override void OnStart() {
        context.SAi.SetDestination(blackboard.moveToPosition);
        context.SAi.lookAtBehavior = lookAtBehavior;
        context.SAi.stopDistance = stoppingDistance;
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.SAi.pathStatus == SAi.PathStatus.Reached)
        {
            return State.Success;
        }
        
        if (context.SAi.NavMeshPath.status == NavMeshPathStatus.PathInvalid || context.SAi.pathStatus == SAi.PathStatus.Failed)
        {
            return State.Failure;
        }

        if (context.SAi.pathStatus == SAi.PathStatus.Pending)
        {
            return State.Running;
        }

        return State.Running;
    }
}
