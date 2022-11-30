using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyerGun : HitscanTestWeapon
{
    protected override void PlayFireEffect()
    {
        if (AudioSource) AudioSource.PlayOneShot(AudioSource.clip);
    }

    protected override void DrawHitscanBulletTrail(Vector3 hitPos)
    {
        foreach (var barrelTransform in barrelTransforms)
        {
            var trailObj = GameManager.Instance.RedLazerPool.Get();
            var lineRenderer = trailObj.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, barrelTransform.position);
            lineRenderer.SetPosition(1, hitPos);
        }
    }
}
