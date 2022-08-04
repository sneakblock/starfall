using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ammoText;

    void Start()
    {
        ammoText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateAmmoCounter(int currentAmmo, int totalAmmo)
    {
        ammoText.text = currentAmmo.ToString() + " / " + totalAmmo.ToString();
    }
}
