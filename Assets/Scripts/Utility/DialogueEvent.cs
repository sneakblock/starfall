using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueEvent", menuName = "Dialogue/DialogueEvent", order = 1)]
public class DialogueEvent : ScriptableObject
{
    [SerializeField] private List<Sprite> sprites;
    private SpriteAnimator animator; // set by dialogue manager
    [SerializeField] private List<string> dialogue;
    
    public List<Sprite> Sprites
    {
        get { return sprites; }
    }

    public SpriteAnimator Animator
    {
        get { return animator; }
        set { animator = value; }
    }

    public List<string> Dialogue
    {
        get { return dialogue; }
    }
}
