using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class TargetHurtAlly : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (context.SAi is not Priestess) return State.Failure;
        SAi bestTarget = AIManager.Instance.activeEnemies[0].GetComponent<SAi>();
        var highestHeuristic = 0f;
        foreach (var allyGameObject in AIManager.Instance.activeEnemies)
        {
            var ally = allyGameObject.GetComponent<SAi>();
            var thisHeuristic = Priestess.EvaluateHealTargetHeuristic(ally);
            if (thisHeuristic > highestHeuristic)
            {
                highestHeuristic = thisHeuristic;
                bestTarget = ally;
            }
        }
        
        context.SAi.SetTargetCharacter(bestTarget);
        Debug.Log($"{context.gameObject.name} has selected {bestTarget.gameObject.name} as target.");
        return State.Success;

    }
}
