using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FMOD.Studio;
using UnityEngine;

public class HitscanTestWeapon : RangedWeapon
{
    protected override void Fire(Vector3 dir)
    {
        var position = barrelTransform.position;
        Ray r = new Ray(position, dir);
        if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
            {
                hit.collider.gameObject.GetComponent<IDamageable>().Damage(weaponData.damage);
            }
        }
        Debug.DrawRay(position, dir * 1000f, Color.red, .5f);
        // Instance = FMODUnity.RuntimeManager.CreateInstance(gunShotEvent);
        // Instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
        // Instance.start();
        // Instance.release();
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
