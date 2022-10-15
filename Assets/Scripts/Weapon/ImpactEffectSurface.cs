using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ImpactEffectSurface : MonoBehaviour
{
    
    public ImpactSurfaceType impactSurfaceType = ImpactSurfaceType.Plaster;

    [System.Serializable]
    public enum ImpactSurfaceType
    {
        Plaster,
        Metal,
        Brick,
        Concrete,
        Glass,
        Dirt,
        Rock,
        Flesh
    }

    

}
