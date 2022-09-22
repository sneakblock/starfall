using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ammoText;

    private bool _isGameManagerNotNull;

    void Awake()
    {
        ammoText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        _isGameManagerNotNull = GameManager.Instance != null;
        UpdateAmmoCounter();
    }

    public void UpdateAmmoCounter()
    {
        if (_isGameManagerNotNull)
        {
            ammoText.text = GameManager.Instance.playerData.currentAmmo.ToString() + " / " + GameManager.Instance.playerData.totalAmmo.ToString();
        }
    }
}
