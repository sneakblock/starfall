using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ammoText;

    void Awake()
    {
        ammoText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        RangedWeapon.OnUpdatePlayerAmmo += UpdateAmmoCounter;
    }

    private void OnDisable()
    {
        RangedWeapon.OnUpdatePlayerAmmo -= UpdateAmmoCounter;
    }

    private void UpdateAmmoCounter(int currAmmo, int maxAmmo)
    {
        ammoText.text = currAmmo.ToString() + " / " + maxAmmo.ToString();
    }
}
