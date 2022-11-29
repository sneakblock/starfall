using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hitmarker : MonoBehaviour
{
    [SerializeField] float markerFadeTime = 0.25f;
    private Image _hitmarkerImage;
    //bool visible = false;
    //float timer;

    // Start is called before the first frame update
    void Start()
    {
        _hitmarkerImage = GetComponent<Image>();
        //_hitmarkerImage.color.a = 0;
        //timer = 0;
        _hitmarkerImage.CrossFadeAlpha(0, 0f, false);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (timer > 0)
        {
            _hitmarkerImage.color.a -= 1 / markerFadeTime;
        }*/
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
        Debug.Log("hitmarker");
        //make it visible
        //_hitmarkerImage.color.a = 1;
        //timer = markerFadeTime;
        _hitmarkerImage.CrossFadeAlpha(1, 0f, false);
        _hitmarkerImage.CrossFadeAlpha(0, markerFadeTime, false);
        //update fade timer
    }
}
