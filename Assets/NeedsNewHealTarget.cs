using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheKiwiCoder;

public class NeedsNewHealTarget : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate()
    {
        if (context.SAi is not Priestess priestess) return State.Failure;
        var target = priestess.HasTargetCharacter() ? priestess.targetChar : null;
        if (target is null)
        {
            SCharacter newTarget = null;
            float shortestDist = Mathf.Infinity;
            foreach (var allyObject in AIManager.Instance.activeEnemies)
            {
                var sAi = allyObject.GetComponent<SAi>();
                if (sAi is Flyer) continue;
                if (sAi is Priestess) continue;
                var dist = Vector3.Distance(priestess.gameObject.transform.position, allyObject.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    newTarget = sAi;
                }
            }

            newTarget ??= (SCharacter) GameManager.Instance.aPlayer;
            priestess.SetTargetCharacter(newTarget);
            return State.Success;
        }

        return State.Failure;
        // {
        //     Debug.Log("target is null");
        //     foreach (var allyObject in AIManager.Instance.activeEnemies.Where(allyObject => allyObject != priestess.gameObject && allyObject.GetComponent<SAi>() is not Flyer))
        //     {
        //         target = allyObject.GetComponent<SAi>();
        //         Debug.Log("target assigned");
        //         break;
        //     }
        // }
        //
        // var scoreTarget = (Priestess.EvaluateHealTargetHeuristic(target), target);
        // foreach (var allyObject in AIManager.Instance.activeEnemies)
        // {
        //     var heuristic = Priestess.EvaluateHealTargetHeuristic(allyObject.GetComponent<SAi>());
        //     if (heuristic > scoreTarget.Item1)
        //     {
        //         scoreTarget = (heuristic, allyObject.GetComponent<SAi>());
        //     }
        // }
        //
        // if (scoreTarget.Item2 == target)
        // {
        //     //Does not need a new target. Keep first target.
        //     if (!priestess.HasTargetCharacter()) priestess.SetTargetCharacter(scoreTarget.Item2);
        //     return State.Failure;
        // }
        //
        // priestess.SetTargetCharacter(scoreTarget.Item2);
        // return State.Success;
    }
}
