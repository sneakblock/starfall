using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkPool : GameObjectPool
{
    protected override void OnReturnedToPool(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        // obj.GetComponentInChildren<LinkMagnet>().ResetTarget();
        base.OnReturnedToPool(obj);
    }
}
