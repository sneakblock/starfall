using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator
{
    Image spriteRenderer;
    List<Sprite> frames;
    float frameRate;
    bool active = false;

    int currentFrame;
    float timer;

    public SpriteAnimator(List<Sprite> frames, Image spriteRenderer, float frameRate = 0.16f)
    {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start() // initializes animation to first frame
    {
        active = true;
        currentFrame = 0;
        timer = 0f;
        spriteRenderer.sprite = frames[0];
    }

    public void HandleUpdate() // plays animation
    {
        if (frames.Count > 0 && active == true) {
            timer += Time.deltaTime;
            if (timer > frameRate)
            {
                currentFrame = (currentFrame + 1) % frames.Count;
                spriteRenderer.sprite = frames[currentFrame];
                timer -= frameRate;
            }
        }
    }
    
    public void Stop() // pauses animation
    {
        active = false;
    }
}
