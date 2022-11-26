using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public enum DaggerAbilityMode {
    Throw,
    Recall
}

public class DaggerAbility : AdvancedAbility
{
    
    [SerializeField] private GameObject dagger;
    [SerializeField] private float totalDamage = 10f;
    [SerializeField] private float bleedDuration = 5f;
    [SerializeField] private int numDaggers = 5;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] public float recallForce = 5f;
    [SerializeField] public float lostDaggerRecoveryTime = 10f;
    [Tooltip("How long after throwing can you recall?")]
    [SerializeField] private float recallCooldown = 1f;
    [Tooltip("Where are the daggers thrown from?")]
    [SerializeField] public Transform handTransform;
    [SerializeField] private float catchRange = 2f;

    private DaggerAbilityMode _mode = DaggerAbilityMode.Throw;
    private List<Dagger> _daggers = new();
    private Dagger _currentRechargingDagger;
    private float _rechargingDaggerTimer = 0f;
    private float _baseCooldown;

    public override void StartAbility()
    {
        for (var i = 0; i < numDaggers; i++)
        {
            var daggerObject = Instantiate(dagger);
            var daggerComponent = daggerObject.GetComponent<Dagger>();
            _daggers.Add(daggerComponent);
            daggerComponent.owner = (Kuze)character;
            daggerComponent.daggerAbility = this;
            daggerComponent.bleedDamage = totalDamage;
            daggerComponent.bleedDuration = bleedDuration;
            daggerObject.SetActive(false);
        }

        _baseCooldown = cooldownTime;
    }

    public override void NotReadyYet()
    {
        if (_mode != DaggerAbilityMode.Recall) return;
        if (_daggers.Any(d => d.daggerState is not (DaggerState.Stuck or DaggerState.Lost)))
        {
            return;
        }

        Debug.Log("Cooldown exemption");
        cooldownTimer = 0f;
        OnCastStarted();
    }

    public override void OnCastStarted()
    {
        var dagsList = new List<Dagger>();
        switch (_mode)
        {
            case DaggerAbilityMode.Throw:
                foreach (var d in _daggers)
                {
                    if (d.IsOutbound()) return;
                    if (d.IsHeld()) dagsList.Add(d);
                }
                ThrowDaggers(dagsList);
                break;
            case DaggerAbilityMode.Recall:
                foreach (var d in _daggers)
                {
                    if (d.IsStuck() || d.IsOutbound()) dagsList.Add(d);
                }
                RecallDaggers(dagsList);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public override void DuringCast()
    {
        
    }

    public override void OnCastEnded()
    {
        base.OnCastEnded();
    }

    private void ThrowDaggers(List<Dagger> daggersToThrow)
    {
        if (daggersToThrow.Count == 0) return;
        if (character is APlayer player)
        {
            player.CallOrientationTimer();
        }
        var handPos = handTransform.position;
        var direction = (character.GetTargetPoint() - handPos).normalized;
        Debug.DrawRay(handPos, direction * 1000f, Color.green, 5f);
        var axis = Vector3.Cross(Vector3.ProjectOnPlane(direction, character.motor.CharacterUp), character.transform.right);
        Debug.DrawRay(handPos, axis * 1000f, Color.blue, 5f);
        var minAngle = Quaternion.AngleAxis(-throwAngle / 2, axis) * direction;
        var maxAngle = Quaternion.AngleAxis(throwAngle / 2, axis) * direction;
        Debug.DrawRay(handPos, minAngle * 1000f, Color.magenta, 5f);
        Debug.DrawRay(handPos, maxAngle * 1000f, Color.magenta, 5f);
        Debug.Log($"Throwing {daggersToThrow.Count} daggers.");
        for (var i = 0; i < daggersToThrow.Count; i++)
        {
            Vector3 adjustedDir;
            adjustedDir = daggersToThrow.Count > 1 ? Vector3.Slerp(minAngle, maxAngle, (float)i / (daggersToThrow.Count - 1)) : direction;
            Debug.DrawRay(handTransform.position, adjustedDir * 1000f, Color.yellow, 5f);
            var daggerObj = daggersToThrow[i].gameObject;
            daggerObj.SetActive(true);
            daggerObj.transform.position = handPos;
            daggerObj.transform.LookAt(handPos + adjustedDir);
            daggersToThrow[i].Throw(adjustedDir * throwForce);
        }

        cooldownTimer = recallCooldown;
        _mode = DaggerAbilityMode.Recall;
    }

    private void RecallDaggers(List<Dagger> daggersToRecall)
    {
        foreach (var d in daggersToRecall)
        {
            d.Recall();
        }

        cooldownTimer = _baseCooldown;
        _mode = DaggerAbilityMode.Throw;
    }

    public float GetCatchRange()
    {
        return catchRange;
    }

    private void Update()
    {
        if (_currentRechargingDagger)
        {
            _rechargingDaggerTimer += Time.deltaTime;
            if (!(_rechargingDaggerTimer >= lostDaggerRecoveryTime)) return;
            _currentRechargingDagger.gameObject.SetActive(true);
            _currentRechargingDagger.Recover();
            _currentRechargingDagger = null;
            _rechargingDaggerTimer = 0f;
        }
        else
        {
            foreach (var d in _daggers.Where(d => d.daggerState == DaggerState.Lost))
            {
                _currentRechargingDagger = d;
                break;
            }
        }
    }
}
