using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StandardBulletWeapon : RangedWeapon
{

    private List<GameObject> _projectiles;

    protected override void Start()
    {
        base.Start();
        _projectiles = new List<GameObject>();
    }

    protected override void Fire(Vector3 dir)
    {
        base.Fire(dir);
        foreach (var barrelTransform in barrelTransforms)
        {
            GameObject projectile = Instantiate(weaponData.bullet, barrelTransform.position, Quaternion.identity);
            projectile.transform.forward = dir.normalized;
            var rb = projectile.GetComponent<Rigidbody>();
            rb.AddForce(dir.normalized * weaponData.firingForce, ForceMode.Impulse);
        }
        // PlayFireEffect();
    }
    
    public void PlayFireEffect()
    {
        foreach (var barrelTransform in barrelTransforms)
        {
            var fireEffectInstance = GameManager.Instance.MuzzleFlashPool.Get();
            fireEffectInstance.transform.position = barrelTransform.position;
            fireEffectInstance.transform.rotation = barrelTransform.rotation;
            fireEffectInstance.transform.parent = barrelTransform;
            if (AudioSource) AudioSource.PlayOneShot(AudioSource.clip);
            StartCoroutine(ReleaseMuzzleFlashWaiter(4f, fireEffectInstance));
        }
    }
    
    IEnumerator ReleaseMuzzleFlashWaiter(float numSeconds, GameObject toRelease)
    {
        yield return new WaitForSeconds(numSeconds);
        GameManager.Instance.MuzzleFlashPool.Release(toRelease);
    }
}