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
    [Tooltip("The amount of damage dealt to an already bleeding enemy, instantly, on recall hit.")]
    [SerializeField] private float burstDamage = 25f;

    [Header("Recovery")]
    [Tooltip("The layers that daggers can get stuck in.")] [SerializeField]
    private LayerMask stickableLayers;

    [Header("Config")]
    public DaggerState daggerState = DaggerState.Held;
    public Kuze owner;
    public DaggerAbility daggerAbility;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private Collider _stuckCollider;
    private static int _enemyLayer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    public void Throw(Vector3 force)
    {
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        daggerState = DaggerState.Outbound;
    }

    public void Recall()
    {
        if (daggerState != DaggerState.Stuck) return;
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
        } else if (collision.gameObject.layer == _enemyLayer)
        {
            SCharacter sCharacter = collision.gameObject.GetComponent<SCharacter>();
            if (!sCharacter.IsBleeding()) sCharacter.StartBleeding(bleedDamage, bleedDuration);
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
    }

    public void Recover()
    {
        daggerState = DaggerState.Held;
        _stuckCollider = null;
        gameObject.SetActive(false);
    }
}
