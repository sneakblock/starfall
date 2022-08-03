using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTestWeapon : RangedWeapon
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Fire(Vector3 dir)
    {
        GameObject projectile = Instantiate(weaponData.bullet, barrelTransform.position, Quaternion.identity);
        projectile.transform.forward = dir.normalized;
        projectile.GetComponent<Rigidbody>().AddForce(dir.normalized * weaponData.firingForce, ForceMode.Impulse);
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
