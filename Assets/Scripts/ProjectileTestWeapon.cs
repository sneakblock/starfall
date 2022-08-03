using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTestWeapon : RangedWeapon
{

    protected override void Fire(Vector3 dir)
    {
        GameObject projectile = Instantiate(weaponData.bullet, barrelTransform.position, Quaternion.identity);
        projectile.tag = gameObject.tag;
        projectile.transform.forward = dir.normalized;
        var rb = projectile.GetComponent<Rigidbody>();
        var coll = projectile.GetComponent<Collider>();
        //This is temp, real functionality should ignore collision with other bullets and the entity that fires the weapon, but not all enemies for example if the weapon is fired by an enemy
        foreach (var g in GameObject.FindGameObjectsWithTag(gameObject.tag))
        {
            if (g.GetComponent<Collider>())
            {
                Physics.IgnoreCollision(coll, g.GetComponent<Collider>(), true);
            }
        }
        rb.AddForce(dir.normalized * weaponData.firingForce, ForceMode.Impulse);
    }

    public override void AnimateAim()
    {
        throw new System.NotImplementedException();
    }

    public override void AnimateFire()
    {
        throw new System.NotImplementedException();
    }

    public override void AnimateReload()
    {
        throw new System.NotImplementedException();
    }

    public override void DoFireEffects()
    {
        throw new System.NotImplementedException();
    }
}
