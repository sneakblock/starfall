using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.VFX;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CloneAbility : AdvancedAbility
{
    [SerializeField] private GameObject clone;
    [SerializeField] private VisualEffect effect;

    private Clone _cloneComponent;
    
    public override void NotReadyYet()
    {
        base.NotReadyYet();
    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        var transform1 = character.transform;
        var position = transform1.position;
        var cloneObject = Instantiate(clone, position, transform1.rotation);
        _cloneComponent = cloneObject.GetComponent<Clone>();
        _cloneComponent.cloneAbility = this;
        _cloneComponent.baseKuze = (Kuze)character;
        var dirToTarget = character.GetTargetPoint() - (position + character.motor.Capsule.center);
        dirToTarget = Vector3.ProjectOnPlane(dirToTarget, Vector3.up);
        var mirrorPlaneNormal = Vector3.Cross(dirToTarget, Vector3.up).normalized;
        _cloneComponent.mirrorNormal = mirrorPlaneNormal;
        Debug.DrawRay(position + character.motor.Capsule.center, _cloneComponent.mirrorNormal, Color.blue, Mathf.Infinity);
        
        if (effect) effect.Play();
        _cloneComponent.SetMaterialization(MaterializationState.Materializing);
    }

    public override void DuringCast()
    {
        base.DuringCast();
    }

    public override void OnCastEnded()
    {
        base.OnCastEnded();
        _cloneComponent.SetMaterialization(MaterializationState.Dematerializing);
    }
}
