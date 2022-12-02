using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.VFX;

public class Priestess : SAi
{
    //Used in evaluating desireability of target.
    private const float LowHealthWeight = 20f;
    private const float FarFromPlayerWeight = .5f;
    private const float MaxConsideredHeuristicDistance = 10f;

    //Higher is more desireable. Forget if this is proper.
    public static float EvaluateHealTargetHeuristic(SAi ally)
    {
        if (ally is Priestess) return 0f;
        if (ally.isTargetedByPriestess) return 0f;
        
        var distFromPlayer = Mathf.Clamp(Vector3.Distance(GameManager.Instance.aPlayer.transform.position, ally.transform.position), 0f, MaxConsideredHeuristicDistance) / MaxConsideredHeuristicDistance;
        var percentageHealth = 1 - ally.health / ally.maxHealth;

        return distFromPlayer * FarFromPlayerWeight + percentageHealth * LowHealthWeight;
    }

    public void CastHeal()
    {
        targetChar = null;
        UseAbility1();
    }

    public override void SetTargetCharacter(SCharacter character)
    {
        if (character is Flyer) return;
        if (HasTargetCharacter())
        {
            if (targetChar is SAi ai)
            {
                ai.isTargetedByPriestess = false;
            }
        }

        base.SetTargetCharacter(character);
        if (character is SAi newAi)
        {
            newAi.isTargetedByPriestess = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GetComponent<HealAbility>().healRadius);
        
        if (!HasTargetCharacter()) return;
        Gizmos.color= Color.green;
        Gizmos.DrawLine(this.transform.position, targetChar.transform.position);
        Gizmos.DrawSphere(targetChar.transform.position, 1f);
    }
}
