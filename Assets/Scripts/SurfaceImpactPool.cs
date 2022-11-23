using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceImpactPool : GameObjectPool
{
    public ImpactEffectSurface.ImpactSurfaceType surfaceType;

    protected override GameObject CreatePooledItem()
    {
        var obj = base.CreatePooledItem();
        var gradient = obj.GetComponentInChildren<FPSShaderColorGradient>();
        gradient.surfaceType = surfaceType;
        return obj;
    }
    
}
