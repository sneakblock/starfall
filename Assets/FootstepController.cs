using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FootstepController : MonoBehaviour
{

    public List<AudioClip> audioClips;
    public AudioSource source;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!source) Debug.LogWarning($"No source for footstep controller on {gameObject.name}");
    }

    public void PlayFootstep()
    {
        if (audioClips.Count == 0) return;
        if (audioClips.Count == 1)
        {
            source.PlayOneShot(audioClips[0]);
        }
        else
        {
            source.PlayOneShot(audioClips[Random.Range(0, audioClips.Count)]);
        }
    }
}
