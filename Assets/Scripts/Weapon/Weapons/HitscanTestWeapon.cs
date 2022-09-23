using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class HitscanTestWeapon : RangedWeapon
{
    protected override void Fire(Vector3 dir)
    {
        base.Fire(dir);
        var position = barrelTransform.position;
        Ray r = new Ray(position, dir);
        if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, OwnerChar.layerMask))
        {
            if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
            {
                hit.collider.gameObject.GetComponent<IDamageable>().Damage(weaponData.damage);
            }
        }
        Debug.DrawRay(position, dir * 1000f, Color.red, .5f);
        PlayFireEffect();
    }
    
}
