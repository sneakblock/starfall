using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class IsWithinRangeOfDestination : ActionNode
{

    public float range;
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (context.SAi is Flyer f)
        {
            return f.HasReachedDestination(range, blackboard.moveToPosition) ? State.Success : State.Failure;
        }
        return (blackboard.moveToPosition - context.SAi.transform.position).magnitude <= range ? State.Success : State.Failure;
    }
}
