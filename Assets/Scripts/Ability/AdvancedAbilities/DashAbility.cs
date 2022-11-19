using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;

public class DashAbility : AdvancedAbility
{
    [SerializeField] private VisualEffect effect;
    //ITS A BLINK NOW!
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private float coolDownReductionPerKill;
    
    Vector3 targetDirection;
    Vector3 movementVector;
    private Animator anim;

    //I set it to a max of 16 because that is the KinematicCharacterMotor's max rigidbody overlap budget
    Collider[] enemiesOverlapped = new Collider[16];

    List<Collider> enemiesToHit = new List<Collider>();
    private static readonly int Dash = Animator.StringToHash("dash");
    //TODO(ben): Add shader stuff during the blink.

    protected override void SetupReferences(SCharacter character)
    {
        anim = character.gameObject.GetComponentInChildren<Animator>();
        if (effect) effect.Stop();
    }

    public override void NotReadyYet()
    {

    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        if (character is APlayer player)
        {
            movementVector = player.orbitCamera.transform.forward * speed;
            player.orientationMethod = APlayer.OrientationMethod.TowardsCamera;
        }
        
        if (anim)
        {
            anim.SetTrigger(Dash);
        }
        
        if (effect) effect.Play();
    }

    public override void DuringCast()
    {
        character.motor.BaseVelocity = movementVector;
        // CheckCollisions();
    }

    public override void OnCastEnded()
    {
        if (character is APlayer player)
        {
            player.orientationMethod = APlayer.OrientationMethod.TowardsMovement;
        }
        
        //Maybe move to a "root motion" approach?
        character.motor.BaseVelocity = Vector3.zero;
        
        // int enemiesKilled = 0;
        //
        // foreach (Collider enemy in enemiesToHit)
        // {
        //     IDamageable damageableEnemy = enemy.gameObject.GetComponent<IDamageable>();
        //     if (damageableEnemy != null)
        //     {
        //         damageableEnemy.Damage(damage * enemiesToHit.Count);
        //         if (damageableEnemy.IsAlive() == false)
        //         {
        //             {
        //                 enemiesKilled++;
        //             }
        //         }
        //     }
        // }
        // enemiesToHit.Clear();
        // cooldownTimer -= enemiesKilled * coolDownReductionPerKill;
    }

    private void CheckCollisions()
    {
        // Discrete collision detection
        int layerMask = 0;
        if (character is SAi)
        {
            layerMask |= (1 << 6);
        }

        if (character is APlayer)
        {
            layerMask |= (1 << 7);
        }
        int numberOfHits = character.motor.CharacterOverlap(
                                 character.motor.TransientPosition,
                                 character.motor.TransientRotation,
                                 enemiesOverlapped,
                                 layerMask,
                                 QueryTriggerInteraction.Ignore);

        for (int i = 0; i < numberOfHits; i++)
        {
            if (enemiesOverlapped[i] != null && enemiesToHit.Contains(enemiesOverlapped[i]) == false)
            {
                enemiesToHit.Add(enemiesOverlapped[i]);
            }
        }
    }
}