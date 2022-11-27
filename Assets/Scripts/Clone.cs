using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Clone : SCharacter
{
    public Kuze baseKuze { get; set; }
    public Vector3 mirrorNormal { get; set; }

    protected override void HandleInputs()
    {
        if (!baseKuze) return;

        if (baseKuze.MoveInputVector != Vector3.zero)
        {
            moveInputVector = Vector3.ClampMagnitude(Vector3.Reflect(baseKuze.MoveInputVector, mirrorNormal), 1f);
            Debug.DrawRay(transform.position, moveInputVector, Color.green, 1f);
        }
        else
        {
            moveInputVector = Vector3.zero;
        }

        var baseKuzeDirToTarget =
            baseKuze.GetTargetPoint() - (baseKuze.transform.position + baseKuze.motor.Capsule.center);
        var reflected = Vector3.Reflect(baseKuzeDirToTarget, mirrorNormal);
        targetPoint = (transform.position + motor.Capsule.center) + reflected;

        if (baseKuze.RewiredPlayer.GetButtonDown("Jump"))
        {
            timeSinceJumpRequested = 0f;
            jumpRequested = true;
        }
        
        if (baseKuze.RewiredPlayer.GetButtonDown("Ability1"))
        {
            UseAbility1();
        }

        if (baseKuze.RewiredPlayer.GetButtonDown("Ability2"))
        {
            UseAbility2();
        }
        
        isAiming = baseKuze.RewiredPlayer.GetButton("Aim");
        isFiring = baseKuze.RewiredPlayer.GetButton("Fire");
        reloadedThisFrame = baseKuze.RewiredPlayer.GetButtonDown("Reload");
    }

    protected override void UpdateCharacter()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(targetPoint, .25f);
    }
}
