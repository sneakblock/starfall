using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class SendAimUpCommand : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        context.SAi.aimStatus = SAi.AimStatus.isUp;
        return State.Success;
    }
}
