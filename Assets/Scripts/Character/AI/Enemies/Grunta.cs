using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grunta : SAi
{
    private Animator _anim;
    private bool _animNull = false;
    private bool _animOff = false;
    
    private static readonly int IsFiring = Animator.StringToHash("isFiring");
    private static readonly int VelX = Animator.StringToHash("velX");
    private static readonly int VelY = Animator.StringToHash("velY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int LookAtPlayer = Animator.StringToHash("lookAtPlayer");
    private static readonly int IsAiming = Animator.StringToHash("isAiming");

    protected override void StartCharacter()
    {
        base.StartCharacter();
        _anim = GetComponentInChildren<Animator>();
        if (_anim == null)
        {
            Debug.LogError($"Grunta {gameObject.name} has no animator!");
            _animNull = true;
        }

        if (_anim.enabled == false)
        {
            _animOff = true;
        }
    }
    
    protected override void UpdateCharacter()
    {
        base.UpdateCharacter();
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (_animNull || _animOff) return;
        
        _anim.SetBool(IsFiring, isFiring);
        _anim.SetBool(IsAiming, isAiming);
        var transform1 = transform;
        var forward = transform1.forward;
        var right = transform1.right;
        _anim.SetFloat(VelX, Vector3.Dot(Vector3.Project(moveInputVector, right), Vector3.right), .2f, Time.deltaTime);
        _anim.SetFloat(VelY, Vector3.Dot(Vector3.Project(moveInputVector, forward), forward), .2f, Time.deltaTime);
        _anim.SetBool(IsMoving, moveInputVector.magnitude > 0.5f);
        _anim.SetBool(LookAtPlayer, lookAtBehavior == LookAtBehavior.AtTargetCharacter);
        
        Debug.DrawRay(transform.position, moveInputVector, Color.white);
        Debug.DrawRay(transform.position, Vector3.Project(moveInputVector, right), Color.red);
        Debug.DrawRay(transform.position, Vector3.Project(moveInputVector, forward), Color.blue);
        Debug.Log($"Velx is {_anim.GetFloat(VelX)}");
        Debug.Log($"Vely is {_anim.GetFloat(VelY)}");
    }
}
