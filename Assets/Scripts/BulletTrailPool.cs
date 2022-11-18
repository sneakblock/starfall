using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrailPool : GameObjectPool
{
    protected override void OnTakeFromPool(GameObject obj)
    {
        base.OnTakeFromPool(obj);
        obj.GetComponent<TrailFade>().color.a = 100;
    }
}
