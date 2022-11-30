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
        foreach (var barrelTransform in barrelTransforms)
        {
            GameObject projectile = Instantiate(weaponData.bullet, barrelTransform.position, Quaternion.identity);
            projectile.tag = gameObject.tag;
            projectile.transform.forward = dir.normalized;
            var rb = projectile.GetComponent<Rigidbody>();
            var coll = projectile.GetComponent<Collider>();
            Debug.Log($"ownerchar is {OwnerChar.gameObject.name}");
            //The bullets collide with the owner of the gun and there is a "recoil" as a result. This is a result of the backend? and that should be solved at a later date.
            Physics.IgnoreCollision(coll, OwnerChar.gameObject.GetComponent<Collider>(), true);
            //This is temp, real functionality should ignore collision with other bullets and the entity that fires the weapon, but not all enemies for example if the weapon is fired by an enemy
            _projectiles.Add(projectile);
            foreach (var p in _projectiles)
            {
                Physics.IgnoreCollision(coll, p.GetComponent<Collider>(), true);
            }
            rb.AddForce(dir.normalized * weaponData.firingForce, ForceMode.Impulse);
        }
        PlayFireEffect();
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