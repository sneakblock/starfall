using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class TrueCastHeal : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.SAi is not Priestess priestess) return State.Failure;
        priestess.CastHeal();
        return State.Success;
    }
}
