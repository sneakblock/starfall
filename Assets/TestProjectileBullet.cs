using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TestProjectileBullet : Bullet
{

    [SerializeField] private float destroyAfterSeconds;
    
    private Rigidbody rb;
    public override void Fire(Vector3 pos, Vector3 dir, float force)
    {
        GameObject projectile = Instantiate(gameObject, pos, Quaternion.identity);
        projectile.transform.forward = dir.normalized;
        rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(dir.normalized * force, ForceMode.Impulse);
    }

    private void OnEnable()
    {
        StartCoroutine(DestroyAfterSeconds(destroyAfterSeconds));
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
