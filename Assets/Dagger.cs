using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public enum DaggerState
{
    Held,
    Outbound,
    Inbound,
    Stuck,
    Lost
}
public class Dagger : MonoBehaviour
{
    [Header("Damage")]
    [Tooltip("The total amount of damage applied by the bleed, over bleedDuration seconds.")]
    [SerializeField] public float bleedDamage = 10f;
    [Tooltip("The number of seconds that the bleed lasts.")]
    [SerializeField] public float bleedDuration = 3f;

    [Header("Recovery")]
    [Tooltip("The layers that daggers can get stuck in.")] [SerializeField]
    private LayerMask stickableLayers;

    [SerializeField] private float autoStickDistance = 1000f;

    [Header("Effects")] public GameObject bleedEffect;
    public GameObject burstEffect;

    [Header("Config")]
    public DaggerState daggerState = DaggerState.Held;
    public SCharacter owner;
    public DaggerAbility daggerAbility;

    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private Collider _stuckCollider;
    private TrailRenderer _trailRenderer;
    private MeshRenderer _meshRenderer;
    private float _defaultTrailTime;

    private SAi _seekTarget;
    [FormerlySerializedAs("_bannedTargets")] public List<SAi> bannedTargets = new();
    public Vector3 initThrownTarget;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _defaultTrailTime = _trailRenderer.time;
        ClearBannedTargets();
    }

    public void Throw(Vector3 force)
    {
        _meshRenderer.enabled = true;
        _rigidbody.isKinematic = false;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("KuzeDagger"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("KuzeDagger"), false);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        ClearSeekTarget();
        ClearBannedTargets();
        daggerState = DaggerState.Outbound;
    }

    public void Recall()
    {
        if (daggerState is not (DaggerState.Stuck or DaggerState.Outbound)) return;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ground"), LayerMask.NameToLayer("KuzeDagger"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Default"), LayerMask.NameToLayer("KuzeDagger"), true);
        Unstick();
        ClearSeekTarget();
        ClearBannedTargets();
        daggerState = DaggerState.Inbound;
    }

    private void Stick(Collider coll)
    {
        _stuckCollider = coll;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = true;
        daggerState = daggerState == DaggerState.Outbound ? DaggerState.Stuck : DaggerState.Lost;
    }

    private void Unstick()
    {
        _rigidbody.isKinematic = false;
    }

    public bool IsHeld()
    {
        return daggerState == DaggerState.Held;
    }

    public bool IsOutbound()
    {
        return daggerState == DaggerState.Outbound;
    }

    public bool IsStuck()
    {
        return daggerState == DaggerState.Stuck;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stickableLayers == (stickableLayers | collision.gameObject.layer) && collision.collider != _stuckCollider)
        {
            Stick(collision.collider);
        }
    }

    private void FixedUpdate()
    {
        if (daggerState == DaggerState.Inbound)
        {
            var position = transform.position;
            var dirToHand = (daggerAbility.handTransform.position - position).normalized;
            transform.LookAt(position + dirToHand);
            _rigidbody.velocity = dirToHand * daggerAbility.recallForce;
            if (Vector3.Distance(daggerAbility.handTransform.position, transform.position) <= daggerAbility.GetCatchRange())
            {
                Recover();
            }
        }

        if (daggerState is DaggerState.Inbound or DaggerState.Outbound)
        {
            if (!HasSeekTarget() && Vector3.Distance(daggerAbility.handTransform.position, transform.position) > daggerAbility.GetCatchRange() * 2f)
            {
                GetNewSeekTarget();
            }
            else
            {
                SeekTarget();
            }
        }

        if (daggerState == DaggerState.Outbound && !HasSeekTarget())
        {
            var dirToInitTarget = (initThrownTarget - transform.position).normalized;
            _rigidbody.velocity = dirToInitTarget * daggerAbility.throwForce;
        }

        //Stick daggers that go into the sky
        if (daggerState is not DaggerState.Outbound) return;
        if (Vector3.Distance(owner.transform.position, transform.position) >= autoStickDistance)
        {
            Stick(null);
        }
    }

    private void GetNewSeekTarget()
    {
        if (!AIManager.Instance)
        {
            Debug.LogWarning("No AI Manager!");
            return;
        }
        
        Debug.DrawRay(transform.position, (Quaternion.AngleAxis(daggerAbility.seekRadius, transform.up) * transform.forward) * daggerAbility.seekConeLength, Color.red, 10f);
        Debug.DrawRay(transform.position, (Quaternion.AngleAxis(-daggerAbility.seekRadius, transform.up) * transform.forward) * daggerAbility.seekConeLength, Color.red, 10f);
        
        foreach (var enemyGameObject in AIManager.Instance.activeEnemies)
        {
            if (!enemyGameObject.TryGetComponent(out SAi sAi)) continue;
            if (!sAi.enabled) continue;
            var transform1 = transform;
            var angleBetween =
                Vector3.Angle(transform1.forward, sAi.motor.Capsule.bounds.center - transform1.position);
            if (Vector3.Distance(transform1.position, sAi.motor.Capsule.bounds.center) >
                daggerAbility.seekConeLength) continue;
            if (!(angleBetween <= daggerAbility.seekRadius)) continue;
            if (bannedTargets.Contains(sAi)) continue;
            _seekTarget = sAi;
            bannedTargets.Add(_seekTarget);
            Debug.Log("Found seek target " + sAi.gameObject.name);
            break;
        }
    }

    private void SeekTarget()
    {
        if (!HasSeekTarget()) return;
        var motorCenter = _seekTarget.motor.Capsule.bounds.center;
        Transform transform1;
        (transform1 = transform).LookAt(motorCenter);
        Debug.Log("Looking at " + motorCenter + "compared to seektarget worldPos " + _seekTarget.gameObject.transform.position);
        _rigidbody.velocity = (motorCenter - transform1.position) * daggerAbility.throwForce;
        Debug.DrawRay(transform1.position, _rigidbody.velocity, Color.magenta, 10f);
    }

    private bool HasSeekTarget()
    {
        return _seekTarget;
    }

    public void ClearBannedTargets()
    {
        bannedTargets.Clear();
    }

    public void ClearSeekTarget()
    {
        _seekTarget = null;
    }

    public void Recover()
    {
        gameObject.SetActive(true);
        daggerState = DaggerState.Held;
        _stuckCollider = null;
        _rigidbody.isKinematic = true;
        _meshRenderer.enabled = false;
        _trailRenderer.time /= 4f;
        StartCoroutine(WaitForTrail());
    }

    IEnumerator WaitForTrail()
    {
        yield return new WaitForSeconds(1f);
        _trailRenderer.Clear();
        _trailRenderer.time = _defaultTrailTime;
        gameObject.SetActive(false);
    }


}
