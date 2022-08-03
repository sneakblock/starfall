using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanTestWeapon : RangedWeapon
{
    protected override void Fire(Vector3 dir)
    {
        Debug.DrawRay(barrelTransform.position, dir * 1000f, Color.red, .5f);
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
