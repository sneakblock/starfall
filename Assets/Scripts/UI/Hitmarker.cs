using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hitmarker : MonoBehaviour
{
    [SerializeField] float markerFadeTime = 0.25;
    private Image _hitmarkerImage;
    //bool visible = false;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        _hitmarkerImage = GetComponent<Image>();
        _hitmarkerImage.color.alpha = 0;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            _hitmarkerImage.color.alpha -= 255 / markerFadeTime;
        }
    }

    private void OnEnable()
    {
        SAi.OnAIHit += UpdateHitmarker;
    }

    private void OnDisable()
    {
        SAi.OnAIHit -= UpdateHitmarker;
    }

    void UpdateHitmarker(object sender, float damage) 
    {
        //make it visible
        _hitmarkerImage.color.alpha = 255;
        timer = markerFadeTime;
        //update fade timer
    }
}
