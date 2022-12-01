using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkBar : MonoBehaviour
{
    private APlayer _player;
    private Slider _slider;

    // Set up each slider's values and set the current Link bar to be composed of Safe Link and Standard Link. 
    void Start()
    {
        _slider = GetComponent<Slider>();

        _player = GameManager.Instance.aPlayer;

    }

    // TEMP: manipulate the Link bar's values.
    void Update()
    {


    }

    public void UpdateLink()
    {
        _slider.value = _player.health / _player.maxHealth;
    }

    public void OnEnable()
    {
        APlayer.OnUpdateLink += UpdateLink;
    }

    public void OnDisable()
    {
        APlayer.OnUpdateLink -= UpdateLink;
    }


}
