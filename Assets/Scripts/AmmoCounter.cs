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

    void Start()
    {
        UpdateAmmoCounter();
    }

    public void UpdateAmmoCounter()
    {
        ammoText.text = GameManager.Instance.playerData.currentAmmo.ToString() + " / " + GameManager.Instance.playerData.totalAmmo.ToString();
    }
}
