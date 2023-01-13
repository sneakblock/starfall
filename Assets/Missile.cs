using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : EnergyBall
{
    public Renderer Renderer;
    public ParticleSystem ParticleSystem;
    public Light Light;
    public AudioClip explosionClip;
    protected override void Explode(Collision collision)
    {
        var effectInstance = Instantiate(impactEffect, transform.position, Quaternion.identity);
        effectInstance.transform.LookAt(collision.contacts[0].normal);
        Destroy(effectInstance, 5f);
        if (explosionClip) AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);

        if (GameManager.Instance.aPlayer)
        {
            var player = GameManager.Instance.aPlayer;
            if (Vector3.Distance(transform.position, player.transform.position) <= explosionRadius)
            {
                player.Damage(damage);
            }
        }

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        if (Renderer) Renderer.enabled = false;
        if (Light) Light.enabled = false;
        if (ParticleSystem) ParticleSystem.Stop();
        
        Destroy(gameObject, 5f);
    }
}
