using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "Dialogue/Event")]
public class DialogueEvent : ScriptableObject
{
    public Sprite sprite;
    public AudioSource audio;
    public string message;
}
