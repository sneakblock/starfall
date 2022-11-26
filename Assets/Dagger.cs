using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
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
    public Kuze owner;
    public DaggerAbility daggerAbility;

    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private Collider _stuckCollider;
    private TrailRenderer _trailRenderer;
    private MeshRenderer _meshRenderer;
    private float _defaultTrailTime;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _defaultTrailTime = _trailRenderer.time;
    }

    public void Throw(Vector3 force)
    {
        _meshRenderer.enabled = true;
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        daggerState = DaggerState.Outbound;
    }

    public void Recall()
    {
        if (daggerState is not (DaggerState.Stuck or DaggerState.Outbound)) return;
        Unstick();
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
            if (Vector3.Distance(owner.transform.position, transform.position) <= daggerAbility.GetCatchRange())
            {
                Recover();
            }
        }

        if (daggerState is not DaggerState.Outbound) return;
        if (Vector3.Distance(owner.transform.position, transform.position) >= autoStickDistance)
        {
            Stick(null);
        }
    }

    public void Recover()
    {
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
