using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHitscanBullet : Bullet
{
    
    public override void Fire(Vector3 pos, Vector3 dir, float force)
    {
        Debug.DrawRay(pos, dir * 1000f, Color.red, .5f);
    }
    
}
