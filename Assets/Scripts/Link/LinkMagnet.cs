using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LinkMagnet : MonoBehaviour
{
    [Range(0, 100)]
    public float magnetRange;

    [Range(1, 50)]
    public float attractForce = 1f;

    private Transform _target = null;
    private LinkDrop _linkDrop;
    private Rigidbody _linkDropRigidbody;
    
    // Start is called before the first frame update
    void Start()
    {
        var coll = GetComponent<SphereCollider>();
        coll.isTrigger = true;
        coll.radius = magnetRange;
        _linkDrop = GetComponentInParent<LinkDrop>();
        _linkDropRigidbody = _linkDrop.GetComponent<Rigidbody>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<APlayer>() != GameManager.Instance.aPlayer) return;
        _target = collision.gameObject.transform;
    }

    private void Update()
    {
        if (!_linkDrop) Destroy(this);
        if (!_target) return;
        // Debug.Log("adding magnet force");
        var dirToPlayer = (_target.position - _linkDrop.gameObject.transform.position).normalized;
        _linkDropRigidbody.AddForce(dirToPlayer * attractForce, ForceMode.Force);
    }
}
