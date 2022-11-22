using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class DaggerAbility : AdvancedAbility
{
    
    [SerializeField] private GameObject dagger;
    [SerializeField] private int numDaggers = 5;
    [SerializeField] private float lostDaggerCooldown = 20f;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private float throwForce = 5f;
    [Tooltip("Where are the daggers thrown from?")]
    [SerializeField] private Transform handTransform;
    
    
    private List<Dagger> _daggers = new();

    public override void StartAbility()
    {
        for (var i = 0; i < numDaggers; i++)
        {
            var daggerObject = Instantiate(dagger);
            var daggerComponent = daggerObject.GetComponent<Dagger>();
            _daggers.Add(daggerComponent);
            daggerComponent.owner = (Kuze)character;
            daggerComponent.daggerAbility = this;
            daggerObject.SetActive(false);
        }
    }

    public override void NotReadyYet()
    {
        
    }

    public override void OnCastStarted()
    {
        var daggersToThrow = new List<Dagger>();
        foreach (var d in _daggers)
        {
            if (d.IsOutbound()) return;
            if (d.IsHeld()) daggersToThrow.Add(d);
        }
        ThrowDaggers(daggersToThrow);
    }

    public override void DuringCast()
    {
        
    }

    public override void OnCastEnded()
    {
        
    }

    private void ThrowDaggers(List<Dagger> daggersToThrow)
    {
        var handPos = handTransform.position;
        var direction = (character.GetTargetPoint() - handPos).normalized;
        Debug.DrawRay(handPos, direction * 5f, Color.green, 5f);
        var axis = Vector3.Cross(direction, character.transform.right);
        Debug.DrawRay(handPos, axis, Color.blue, 5f);
        var minAngle = Quaternion.AngleAxis(-throwAngle / 2, axis) * direction;
        var maxAngle = Quaternion.AngleAxis(throwAngle / 2, axis) * direction;
        Debug.DrawRay(handPos, minAngle * 5f, Color.magenta, 5f);
        Debug.DrawRay(handPos, maxAngle * 5f, Color.magenta, 5f);
        Debug.Log($"Throwing {daggersToThrow.Count} daggers.");
        for (var i = 0; i < daggersToThrow.Count; i++)
        {
            var adjustedDir = Vector3.Slerp(minAngle, maxAngle, (float)i / (daggersToThrow.Count - 1));
            Debug.DrawRay(handTransform.position, adjustedDir, Color.yellow, 5f);
            var daggerObj = daggersToThrow[i].gameObject;
            daggerObj.SetActive(true);
            daggerObj.transform.position = handPos;
            daggerObj.transform.LookAt(handPos + adjustedDir);
            daggersToThrow[i].Throw(adjustedDir * throwForce);
        }
    }
}
