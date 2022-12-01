using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class HealAbilityIsReady : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        return context.SAi.GetComponent<HealAbility>().IsReady() ? State.Success : State.Failure;
    }
}
