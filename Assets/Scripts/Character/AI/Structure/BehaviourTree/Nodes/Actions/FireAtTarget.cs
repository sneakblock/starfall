using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class FireAtTarget : ActionNode
{
    public bool overrideCharacterDefaultAccuracy = false;
    [Range(0f, 1f)]
    public float accuracy;
    
    [Tooltip("Use a random amount of seconds between the specified lower and upper range if true. If false, " +
             "uses the seconds to hold down trigger variable.")]
    public bool useSecondsRange = false;
    
    public float secondsToHoldDownTrigger = .5f;

    public float minSecondsToHoldDownTrigger = .2f;
    public float maxSecondsToHoldDownTrigger = 5f;

    private float _startTimeStamp;
    private float _oldAccuracy;

    protected override void OnStart()
    {
        _oldAccuracy = context.SAi.accuracy;
        context.SAi.lookAtBehavior = SAi.LookAtBehavior.AtTargetCharacter;
        if (useSecondsRange)
            secondsToHoldDownTrigger = Random.Range(minSecondsToHoldDownTrigger, maxSecondsToHoldDownTrigger);
        if (overrideCharacterDefaultAccuracy) context.SAi.accuracy = accuracy;
        _startTimeStamp = Time.time;
    }

    protected override void OnStop()
    {
        context.SAi.triggerStatus = SAi.TriggerStatus.isUp;
        context.SAi.accuracy = _oldAccuracy;
    }

    protected override State OnUpdate()
    {
        //SAi does not have a targetCharacter or targetPoint.
        if (!context.SAi.GetRangedWeapon()) return State.Failure;

        //Either we have reached the end of the chosen number of seconds, or we have started a reload.
        if (Time.time - secondsToHoldDownTrigger > _startTimeStamp || context.SAi.GetRangedWeapon().GetReloading())
            return State.Success;
        
        context.SAi.triggerStatus = SAi.TriggerStatus.isHeld;
        return State.Running;
    }
}
