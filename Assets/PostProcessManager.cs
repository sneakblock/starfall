using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessManager : MonoBehaviour
{
    public static PostProcessManager Instance { get; private set; }

    //This is the volume used for level post processing.
    public Volume standardVolume;

    public Volume effectVolume;

    private void Awake()
    {
        //Initialize the instance of the Game Manager.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void SetStandardVolumeProfile(VolumeProfile volumeProfile)
    {
        standardVolume.profile = volumeProfile;
    }

    public void LerpToEffect(float seconds)
    {
        StartCoroutine(fadeVolume(standardVolume, 1f, 0f, seconds));
        StartCoroutine(fadeVolume(effectVolume, 0f, 1f, seconds));
    }
    
    public void LerpFromEffect(float seconds, VolumeProfile newProfile)
    {
        standardVolume.profile = newProfile;
        StartCoroutine(fadeVolume(standardVolume, 0f, 1f, seconds));
        StartCoroutine(fadeVolume(effectVolume, 1f, 0f, seconds));
    }
    
    IEnumerator fadeVolume(Volume volumeToFade, float startWeight, float endWeight, float duration) {
        float startTime = Time.time;
     
        while(true) {
                   
            if(duration == 0) {
                volumeToFade.weight = endWeight;
                break;//break, to prevent division by  zero
            }
            float elapsed = Time.time - startTime;
               
            volumeToFade.weight = Mathf.Clamp01(Mathf.Lerp( startWeight,
                endWeight,
                elapsed/duration ));
     
            if(volumeToFade.weight == endWeight) {
                break;
            }
            yield return null;
        }//end while
    }
}
