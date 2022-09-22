using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class HealthBar : MonoBehaviour
{
    public Slider _healthSlider;

    void Start()
    {
        
    }

    public void SetHealthBar(float health)
    {
        _healthSlider.value = health;
    }

    void SetHealthBarMax(float maxHealth)
    {
        _healthSlider.maxValue = maxHealth;
        _healthSlider.value = maxHealth;
    }
}
