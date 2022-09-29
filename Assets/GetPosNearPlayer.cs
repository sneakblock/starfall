using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class GetPosNearPlayer : ActionNode
{

    public float range;
    
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (!context.aPlayer) return State.Failure;
        var pt = Random.insideUnitCircle * range;
        var playerPos = context.aPlayer.transform.position;
        var randomPos = new Vector3(playerPos.x + pt.x,
            playerPos.y, playerPos.z + pt.y);
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Default");
        var r = new Ray(randomPos, Vector3.down);
        blackboard.moveToPosition = Physics.Raycast(r, out var hit, Mathf.Infinity, layerMask) ? hit.point : randomPos;
        return State.Success;
    }
}
