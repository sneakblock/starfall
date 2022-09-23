using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private TextMeshProUGUI message;
    public DialogueEvent[] dialogueEvents;

    void Start() {
        message.text = dialogueEvents[0].message;
        image.sprite = dialogueEvents[0].sprite;
        Debug.Log(dialogueEvents[0].message);
    }
    // data to collect before clip position to get loudness
    private static int sampleWindow = 64;

    private static float getLoudnessFromAudio(int clipPosition, AudioClip clip) {
        int startPosition = clipPosition - sampleWindow;
        if (startPosition < 0) {
            return 0;
        }

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        // compute loudness
        float totalLoudness = 0;

        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]);
        }

        return totalLoudness / sampleWindow;
    }
}
