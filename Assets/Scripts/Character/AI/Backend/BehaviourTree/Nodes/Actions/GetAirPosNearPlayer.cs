using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class GetAirPosNearPlayer : ActionNode
{

    public float range;
    
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (!context.aPlayer) return State.Failure;
        var pt = Random.insideUnitSphere * range;
        var playerPos = context.aPlayer.transform.position;
        var randomPos = new Vector3(playerPos.x + pt.x,
            playerPos.y + pt.y, playerPos.z + pt.z);
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Default");
        var r = new Ray(randomPos, playerPos - randomPos);
        //Return the random point around the player if it has LoS to them, otherwise, set the goal pos to the current pos.
        blackboard.moveToPosition = Physics.Raycast(r, out var hit, Mathf.Infinity, layerMask) ? hit.point : context.SAi.transform.position;
        return State.Success;
    }
}
