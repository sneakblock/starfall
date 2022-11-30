using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class PathComplete : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        return context.SAi.pathStatus == SAi.PathStatus.Reached ? State.Success : State.Failure;
    }
}
