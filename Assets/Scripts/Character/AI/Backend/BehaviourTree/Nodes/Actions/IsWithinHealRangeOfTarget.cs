using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public enum EvaluatedPosition
{
    Priestess,
    Destination
}
public class IsWithinHealRangeOfTarget : ActionNode
{

    public EvaluatedPosition positionToEval = EvaluatedPosition.Priestess;
    public float reduceRealRadius = 2f;
    
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.SAi is not Priestess priestess) return State.Failure;
        if (priestess.HasTargetCharacter())
        {
            var evalPoint = positionToEval == EvaluatedPosition.Priestess
                ? priestess.motor.Capsule.bounds.center
                : blackboard.moveToPosition;
            return Vector3.Distance(evalPoint,
                priestess.targetChar.motor.Capsule.bounds.center) <= (priestess.healRadius - reduceRealRadius)
                ? State.Success
                : State.Failure;
        }

        return State.Failure;
    }
}
