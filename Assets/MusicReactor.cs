// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class MusicReactor : MonoBehaviour
// {
//     public Renderer Renderer;
//     private Material _material;
//
//     [SerializeField] private float minVertRes = 5f;
//     [SerializeField] private float maxVertRes = 300f;
//     
//     [SerializeField] private float minVertDispl = 0f;
//     [SerializeField] private float maxVertDispl = .2f;
//     
//     [SerializeField] private float minDistortSpeed = .1f;
//     [SerializeField] private float maxDistortSpeed = 3f;
//     
//     [SerializeField] private float minColorBalance = 0f;
//     [SerializeField] private float maxColorBalance = 1f;
//     
//     private static readonly int VertexResolution = Shader.PropertyToID("Vector1_B2CC132F");
//     private static readonly int VertexDisplacmentAmount = Shader.PropertyToID("_VertexDisplacementAmount");
//     private static readonly int DistortionColorBalance = Shader.PropertyToID("_DistortionColorBalance");
//     private static readonly int HeatDistortionSpeed = Shader.PropertyToID("_HeatDistortionSpeed");
//
//     private void Update()
//     {
//         _material = Renderer.material;
//         var lowBand = AudioSpectrum.audioBandBuffer[1];
//         var midBand = AudioSpectrum.audioBandBuffer[4];
//         var highBand = AudioSpectrum.audioBandBuffer[7];
//         
//         _material.SetFloat(VertexResolution, Mathf.Lerp(minVertRes, maxVertRes, lowBand));
//         _material.SetVector(HeatDistortionSpeed, new Vector4(Mathf.Lerp(minDistortSpeed, maxDistortSpeed, midBand), Mathf.Lerp(minDistortSpeed, maxDistortSpeed, midBand)));
//         _material.SetFloat(DistortionColorBalance, Mathf.Lerp(minColorBalance, maxColorBalance, midBand));
//         _material.SetFloat(VertexDisplacmentAmount, Mathf.Lerp(minVertDispl, maxVertDispl, highBand));
//     }
// }

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicReactor : MonoBehaviour
{
    // What band does this reactor listen to?
    [Range(0, 7)] public int syncBand;
    
    [Tooltip("All possible reactor facets for this reactor. This does not mean that they are active.")]
    public ReactorFacet[] ReactorFacets;

    [HideInInspector]
    public List<Material> mats = new();
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var reactorFacet in ReactorFacets)
        {
            reactorFacet.Owner = this;
        }
        AcquireMaterials();
    }

    public void AcquireMaterials()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                mats.Add(material);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var reactorFacet in ReactorFacets)
        {
            if (reactorFacet.IsActive) reactorFacet.Tick();
        }
    }
}


