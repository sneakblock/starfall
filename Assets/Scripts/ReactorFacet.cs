using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReactorFacet
{
    public Parameter Param;

    public bool useOwnerSyncBand = true;
    [Range(0, 7)] public int syncBand;
    
    public float MinValue;
    public float MaxValue;

    public bool IsActive = true;
    public MusicReactor Owner;

    private static readonly int VertexResolution = Shader.PropertyToID("Vector1_B2CC132F");
    private static readonly int VertexDisplacmentAmount = Shader.PropertyToID("_VertexDisplacementAmount");
    private static readonly int DistortionColorBalance = Shader.PropertyToID("_DistortionColorBalance");
    private static readonly int HeatDistortionSpeed = Shader.PropertyToID("_HeatDistortionSpeed");

    public enum Parameter
    {
        VertexResolution,
        VertexDisplacement,
        DistortionColorBalance,
        HeatDistortionSpeed
    }

    public void Tick()
    {
        if (!IsActive)
        {
            SetActive(false);
            return;
        }
        var audioVal = AudioSpectrum.audioBandBuffer[useOwnerSyncBand ? Owner.syncBand : syncBand];
        var paramVal = Mathf.Lerp(MinValue, MaxValue, audioVal);
        switch (Param)
        {
            case Parameter.VertexResolution:
                UpdateFloatValue(VertexResolution, paramVal);
                break;
            case Parameter.VertexDisplacement:
                UpdateFloatValue(VertexDisplacmentAmount, paramVal);
                break;
            case Parameter.DistortionColorBalance:
                UpdateFloatValue(DistortionColorBalance, paramVal);
                break;
            case Parameter.HeatDistortionSpeed:
                UpdateVec4Value(HeatDistortionSpeed, paramVal);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateFloatValue(int valueID, float paramVal)
    {
        foreach (var mat in Owner.mats)
        {
            mat.SetFloat(valueID, paramVal);
        }
    }

    private void UpdateVec4Value(int valueID, float paramVal)
    {
        foreach (var mat in Owner.mats)
        {
            mat.SetVector(valueID, new Vector4(paramVal, paramVal));
        }
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }
    
    
}
