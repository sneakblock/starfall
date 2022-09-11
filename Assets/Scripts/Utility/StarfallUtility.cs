using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StarfallUtility
{
    public static float RandGaussian(float stddev, float mean = 0f)
    {
        float v0 = 1f - Random.Range(0f, 1f);
        float v1 = 1f - Random.Range(0f, 1f);
        float randNorm = Mathf.Sqrt(-2f * Mathf.Log(v0)) * Mathf.Sin(2f * Mathf.PI * v1);
        return mean + stddev * randNorm;
    }
}

