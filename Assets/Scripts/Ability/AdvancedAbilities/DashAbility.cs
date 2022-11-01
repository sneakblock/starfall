using System;
using UnityEngine;
using System.Collections.Generic;

public class DashAbility : AdvancedAbility
{
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private float coolDownReductionPerKill;
    
    Vector3 targetDirection;
    Vector3 movementVector;

    //I set it to a max of 16 because that is the KinematicCharacterMotor's max rigidbody overlap budget
    Collider[] enemiesOverlapped = new Collider[16];

    List<Collider> enemiesToHit = new List<Collider>();
    private static readonly int Dash = Animator.StringToHash("dash");
    private static readonly int VertexJitter = Shader.PropertyToID("_useVertexJitter");

    public override void NotReadyYet()
    {

    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        movementVector = character.GetTargetMovementDirection() * speed;
        if (character is SAi)
        {
            //6 is the player layer
            character.motor.CollidableLayers &= ~(1 << 6);
        }

        if (character is APlayer player)
        {
            //7 is the Enemy layer
            player.motor.CollidableLayers &= ~(1 << 7);

            //if the player isn't inputting a direction
            if (movementVector == Vector3.zero)
            {
                movementVector = player.orbitCamera.transform.forward.normalized;
                movementVector.y = 0;

                //if the player is looking straight up or straight down
                if (movementVector == Vector3.zero)
                {
                    movementVector = player.motor.CharacterForward.normalized;
                }
                player.motor.SetRotation(Quaternion.LookRotation(movementVector, player.motor.CharacterUp));
                movementVector *= speed;
            }
        }

        var anim = character.gameObject.GetComponentInChildren<Animator>();
        var renderers = GameObject.Find("kuze_anims").GetComponentsInChildren<Renderer>();
        if (anim)
        {
            anim.SetTrigger(Dash);
        }
        else
        {
            Debug.Log("No animator");
        }

        // foreach (var renderer in renderers)
        // {
        //     renderer.material.SetFloat(VertexJitter, 1);
        // }
    }

    public override void DuringCast()
    {
        character.motor.BaseVelocity = movementVector;
        CheckCollisions();
    }

    public override void OnCastEnded()
    {
        if (character is SAi)
        {
            //6 is the player layer
            character.motor.CollidableLayers |= (1 << 6);
        }

        if (character is APlayer)
        {
            //7 is the Enemy layer
            character.motor.CollidableLayers |= (1 << 7);
        }

        int enemiesKilled = 0;

        foreach (Collider enemy in enemiesToHit)
        {
            IDamageable damageableEnemy = enemy.gameObject.GetComponent<IDamageable>();
            if (damageableEnemy != null)
            {
                damageableEnemy.Damage(damage * enemiesToHit.Count);
                if (damageableEnemy.IsAlive() == false)
                {
                    {
                        enemiesKilled++;
                    }
                }
            }
        }

        enemiesToHit.Clear();

        cooldownTimer -= enemiesKilled * coolDownReductionPerKill;
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