using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class HasNoPath : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        return context.SAi.pathStatus == SAi.PathStatus.None ? State.Success : State.Failure;
    }
}
