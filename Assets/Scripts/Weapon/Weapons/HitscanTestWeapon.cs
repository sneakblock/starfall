using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class HitscanTestWeapon : RangedWeapon
{
    [SerializeField] private GameObject hitscanBulletTrail;

    protected override void Fire(Vector3 dir)
    {
        base.Fire(dir);
        var position = barrelTransform.position;
        Ray r = new Ray(position, dir);
        Vector3 hitPoint;
        if (Physics.Raycast(r, out RaycastHit hit, Mathf.Infinity, OwnerChar.layerMask))
        {
            if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
            {
                hit.collider.gameObject.GetComponent<IDamageable>().Damage(weaponData.damage);
            }
            HandleImpactEffects(hit.collider.gameObject, hit);
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = barrelTransform.forward * 1000f;
        }
        Debug.DrawRay(position, dir * 1000f, Color.red, .5f);
        PlayFireEffect();
        DrawHitscanBulletTrail(hitPoint);
    }

    private void DrawHitscanBulletTrail(Vector3 hitPos)
    {
        if (!hitscanBulletTrail) return;
        
        var trailObj = Instantiate(hitscanBulletTrail, barrelTransform.position, Quaternion.identity);
        var lineRenderer = trailObj.GetComponent<LineRenderer>();
        
        if (!lineRenderer) return;
        
        lineRenderer.SetPosition(0, barrelTransform.position);
        lineRenderer.SetPosition(1, hitPos);
        Destroy(trailObj, 1f);
    }

}
