using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CloneAbility : AdvancedAbility
{
    [SerializeField] private GameObject clone;
    
    public override void NotReadyYet()
    {
        base.NotReadyYet();
    }

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        var position = character.transform.position;
        var cloneObject = Instantiate(clone, position, Quaternion.identity);
        cloneObject.transform.rotation = character.transform.rotation;
        var cloneComponent = cloneObject.GetComponent<Clone>();
        cloneComponent.baseKuze = (Kuze)character;
        var dirToTarget = character.GetTargetPoint() - (position + character.motor.Capsule.center);
        dirToTarget = Vector3.ProjectOnPlane(dirToTarget, Vector3.up);
        var mirrorPlaneNormal = Vector3.Cross(dirToTarget, Vector3.up).normalized;
        cloneComponent.mirrorNormal = mirrorPlaneNormal;
        Debug.DrawRay(position + character.motor.Capsule.center, cloneComponent.mirrorNormal, Color.blue, Mathf.Infinity);
    }

    public override void DuringCast()
    {
        base.DuringCast();
    }

    public override void OnCastEnded()
    {
        base.OnCastEnded();
    }
}
