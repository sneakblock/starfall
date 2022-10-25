using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class IsWithinRangeOfPlayer : ActionNode
{
    public ObjectToCheck objectToCheck;
    public float range;

    public enum ObjectToCheck
    {
        SAi,
        MoveToPosition
    }

    private Vector3 _vectorToPlayer;
    protected override void OnStart()
    {
        Vector3 objectPosition;
        switch (objectToCheck)
        {
            case ObjectToCheck.SAi:
                objectPosition = context.SAi.Collider.bounds.center;
                break;
            case ObjectToCheck.MoveToPosition:
                objectPosition = blackboard.moveToPosition;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (context.aPlayer) _vectorToPlayer = context.aPlayer.Collider.bounds.center - objectPosition;
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (!context.aPlayer) return State.Failure;
        return _vectorToPlayer.magnitude <= range ? State.Success : State.Failure;
    }
}
