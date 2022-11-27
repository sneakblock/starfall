using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneBulletTrailPool : GameObjectPool
{
    protected override void OnTakeFromPool(GameObject obj)
    {
        base.OnTakeFromPool(obj);
        obj.GetComponent<CloneTrailFade>().color.a = 100;
    }
}
