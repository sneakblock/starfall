using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ImpactEffect
{
    public ImpactEffectSurface.ImpactSurfaceType SurfaceType;
    public GameObject[] Effects;
    public AudioClip[] ImpactClips;
}
