using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class LinkDrop : MonoBehaviour
{
    
    [Range(0, 100)]
    public float value;

    public float absorbRadius = 10f;
    public float absorbDuration = 1f;

    private Material _mat;
    private Rigidbody _rb;
    private SphereCollider _coll;

    public static readonly int AbsorbID = Shader.PropertyToID("_Absorb");
    public static readonly int AbsorbToPosID = Shader.PropertyToID("_AbsorbToPos");
    public static readonly int AbsorbCompletionID = Shader.PropertyToID("_AbsorbCompletion");

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<SphereCollider>();
    }

    private void OnEnable()
    {
        _mat = GetComponent<Renderer>().material;
        _mat.SetInt(AbsorbID, 0);
        _mat.SetFloat(AbsorbCompletionID, 0);
    }

    private void Update()
    {
        if (!GameManager.Instance) return;
        if (!(Vector3.Distance(transform.position, GameManager.Instance.aPlayer.gameObject.transform.position) <=
              absorbRadius)) return;
        StartCoroutine(Absorb(absorbDuration));
    }

    IEnumerator Absorb(float duration)
    {
        float startTime = Time.time;
        _mat.SetInt(AbsorbID, 1);
        while(true) {
            float elapsed = Time.time - startTime;
            _mat.SetVector(AbsorbToPosID, GameManager.Instance.aPlayer.motor.Capsule.bounds.center);
            _mat.SetFloat(AbsorbCompletionID, Mathf.Clamp01(Mathf.Lerp(0,
                1,
                elapsed / duration)));
     
            if(_mat.GetFloat(AbsorbCompletionID) >= .99f) {
                break;
            }
            yield return null;
        }//end while
        GameManager.Instance.aPlayer.Heal(value);
        GameManager.Instance.LinkAudioSource.Play();
        GameManager.Instance.LinkPool.Release(gameObject);
    }

}
