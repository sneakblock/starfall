using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnergyBall : MonoBehaviour
{
    public GameObject impactEffect;
    public float explosionRadius;
    public float damage;

    private void OnCollisionEnter(Collision collision)
    {
        Explode(collision);
    }

    protected virtual void Explode(Collision collision)
    {
        var effectInstance = Instantiate(impactEffect, transform.position, Quaternion.identity);
        Destroy(effectInstance, 1f);

        if (GameManager.Instance.aPlayer)
        {
            var player = GameManager.Instance.aPlayer;
            if (Vector3.Distance(transform.position, player.transform.position) <= explosionRadius)
            {
                player.Damage(damage);
            }
        }
        
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
