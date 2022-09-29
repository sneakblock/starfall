using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using UnityEngine.AI;

public class SendMoveToPositionCommand : ActionNode
{
    public SAi.LookAtBehavior lookAtBehavior = SAi.LookAtBehavior.AtPath;

    protected override void OnStart() {
        context.SAi.SetDestination(blackboard.moveToPosition);
        context.SAi.lookAtBehavior = lookAtBehavior;
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.SAi.NavMeshPath.status == NavMeshPathStatus.PathInvalid || context.SAi.pathStatus == SAi.PathStatus.Failed)
        {
            return State.Failure;
        }

        return State.Success;
    }
}
