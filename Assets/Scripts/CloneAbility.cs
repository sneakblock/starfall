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

    private GameObject _cloneObject;
    private Clone _cloneComponent;
    
    public override void NotReadyYet()
    {
        base.NotReadyYet();
    }

    protected override void SetupReferences(SCharacter character)
    {
        base.SetupReferences(character);
        var transform1 = character.transform;
        _cloneObject = Instantiate(clone, transform1.position, transform1.rotation);
        _cloneComponent = _cloneObject.GetComponent<Clone>();
        _cloneComponent.cloneAbility = this;
        _cloneComponent.baseKuze = (Kuze)character;
        _cloneObject.SetActive(false);
    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        _cloneComponent.SetMaterialization(MaterializationState.Materializing);
        var transform1 = character.transform;
        var position = transform1.position;
        _cloneObject.SetActive(true);
        if (_cloneComponent.CloneAudioSource) _cloneComponent.CloneAudioSource.Play();
        _cloneComponent.motor.SetPosition(position);
        _cloneComponent.motor.SetRotation(transform1.rotation);
        var dirToTarget = character.GetTargetPoint() - (position + character.motor.Capsule.center);
        dirToTarget = Vector3.ProjectOnPlane(dirToTarget, Vector3.up);
        var mirrorPlaneNormal = Vector3.Cross(dirToTarget, Vector3.up).normalized;
        _cloneComponent.mirrorNormal = mirrorPlaneNormal;
        Debug.DrawRay(position + character.motor.Capsule.center, _cloneComponent.mirrorNormal, Color.blue, Mathf.Infinity);
        
        if (effect) effect.Play();
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
