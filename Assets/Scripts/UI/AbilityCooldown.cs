using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class AbilityCooldown: MonoBehaviour
{
    [SerializeField]
    [Tooltip("1: Dash, 2: Dagger, 3: Clone")]
    public int abilityNumber;

    private APlayer _player;

    private Slider _slider;

    private AdvancedAbility _currAbility;

    // Set up each slider's values and set the current Link bar to be composed of Safe Link and Standard Link. 
    void Start()
    {
        _slider = GetComponent<Slider>();

        _player = GameManager.Instance.aPlayer;

        if (abilityNumber == 1)
        {
            _currAbility = _player.GetComponent<DashAbility>();
        }
        else if (abilityNumber == 2)
        {
            _currAbility = _player.GetComponent<DaggerAbility>();
        }
        else if (abilityNumber == 3)
        {
            _currAbility = _player.GetComponent<CloneAbility>();
        }

    }

    void Update()
    {
        if (_currAbility.castCompleted)
        {
            UpdateSliderCooldown();
        }
        else
        {
            UpdateSliderCast();
        }
        
    }

    public void UpdateSliderCooldown()
    {
        _slider.value = 1 - (_currAbility.cooldownTimer / _currAbility.cooldownTime);
    }

    public void UpdateSliderCast()
    {
        _slider.value = _currAbility.castTimer / _currAbility.castTime;

    }

}
