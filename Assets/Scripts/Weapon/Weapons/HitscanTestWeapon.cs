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
        Vector3 hitPoint = new Vector3();
        foreach (var barrelTransform in barrelTransforms)
        {
            var position = barrelTransform.position;
            Ray r = new Ray(position, dir);
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
                hitPoint = barrelTransform.position + dir * 100f;
            }
            Debug.DrawLine(position, hitPoint, Color.red, 2f);
        }
        PlayFireEffect();
        DrawHitscanBulletTrail(hitPoint);
    }

    protected virtual void PlayFireEffect()
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

    private IEnumerator ReleaseMuzzleFlashWaiter(float numSeconds, GameObject toRelease)
    {
        yield return new WaitForSeconds(numSeconds);
        GameManager.Instance.MuzzleFlashPool.Release(toRelease);
    }

    protected virtual void DrawHitscanBulletTrail(Vector3 hitPos)
    {
        foreach (var barrelTransform in barrelTransforms)
        {
            var trailObj = GameManager.Instance.BulletTrailPool.Get();
            var lineRenderer = trailObj.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, barrelTransform.position);
            lineRenderer.SetPosition(1, hitPos);
        }
    }

}
