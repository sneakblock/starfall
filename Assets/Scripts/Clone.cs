using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CloneOrientation
{
    TowardsMovement,
    TowardsTarget
}

public class Clone : SCharacter
{
    public Kuze baseKuze { get; set; }
    public Vector3 mirrorNormal { get; set; }

    public CloneOrientation cloneOrientation { get; set; } = CloneOrientation.TowardsMovement;

    public Animator anim;
    
    private static readonly int IsFiring = Animator.StringToHash("isFiring");
    private static readonly int VelX = Animator.StringToHash("velX");
    private static readonly int VelY = Animator.StringToHash("velY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int LookAtCamera = Animator.StringToHash("lookAtCamera");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");
    private static readonly int InAir = Animator.StringToHash("inAir");
    private static readonly int DistToGround = Animator.StringToHash("distToGround");
    private static readonly int JumpDown = Animator.StringToHash("jumpDown");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");

    protected override void StartCharacter()
    {
        base.StartCharacter();
        anim = GetComponentInChildren<Animator>();
        baseKuze.orbitCamera.IgnoredColliders.Add(GetComponentInChildren<Collider>());
    }

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
        var center = motor.Capsule.center;
        var position = transform.position;
        targetPoint = position + center + reflected;

        switch (cloneOrientation)
        {
            case CloneOrientation.TowardsMovement:
                lookInputVector = moveInputVector.normalized;
                break;
            case CloneOrientation.TowardsTarget:
                lookInputVector = (targetPoint - Collider.bounds.center);
                lookInputVector.y = 0f;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

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
        
        HandleAnimationInputs();
    }

    protected override void AimDown()
    {
        base.AimDown();
        cloneOrientation = CloneOrientation.TowardsTarget;
    }

    protected override void AimUp()
    {
        base.AimUp();
        cloneOrientation = CloneOrientation.TowardsMovement;
    }

    private void HandleAnimationInputs()
    {
        if (!anim) return;
        anim.SetBool(IsFiring, isFiring);
        anim.SetBool(IsAiming, isAiming);
        if (jumpRequested && !_jumpConsumed)
        {
            anim.SetTrigger(JumpDown);
        }
        
        anim.SetBool(InAir, !motor.GroundingStatus.IsStableOnGround);
        anim.SetBool(IsFalling, !motor.GroundingStatus.IsStableOnGround && motor.Velocity.y <= -.05f);
        if (anim.GetBool(InAir) || anim.GetBool(IsFalling))
        {
            Ray r = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(r, out var hit, 500f, 1 << LayerMask.NameToLayer("Default")))
            {
                anim.SetFloat(DistToGround, hit.distance);
            }
        }
        anim.SetFloat(VelX,  moveInputVector.x, .05f, Time.deltaTime);
        anim.SetFloat(VelY, moveInputVector.z, .05f, Time.deltaTime);
        anim.SetBool(IsMoving, moveInputVector.magnitude > 0.5f);
        anim.SetBool(LookAtCamera, cloneOrientation == CloneOrientation.TowardsTarget);
    }

    protected override void RequestFirePrimary()
    {
        base.RequestFirePrimary();
        if (!_weapon.GetReloading())
        {
            StartCoroutine(OrientationTimer(baseKuze.secondsToLockShootingOrientation));
        }
    }

    protected override void UpdateCharacter()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(targetPoint, .25f);
    }
    
    private IEnumerator OrientationTimer(float duration)
    {
        cloneOrientation = CloneOrientation.TowardsTarget;
        yield return new WaitForSeconds(duration);
        if (!isAiming && Time.time - _weapon.GetTimeLastFired() >= duration - .1f)
            cloneOrientation = CloneOrientation.TowardsMovement;
    }
}
