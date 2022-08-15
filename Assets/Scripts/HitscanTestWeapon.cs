using System.Collections;
using System.Collections.Generic;
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
        EventEmitter.Play();
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
