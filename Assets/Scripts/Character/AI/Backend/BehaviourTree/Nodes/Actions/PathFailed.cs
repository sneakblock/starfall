using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class PathFailed : ActionNode
{
    protected override void OnStart()
    {
        
    }

    protected override void OnStop()
    {
        
    }

    protected override State OnUpdate()
    {
        return context.SAi.pathStatus == SAi.PathStatus.Failed ? State.Success : State.Failure;
    }
}
