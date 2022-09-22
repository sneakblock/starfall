using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{

    private RectTransform _crosshairRectTransform;

    [SerializeField]
    [Range(55f, 600f)]
    private float size;

    private bool _isGameManagerNotNull;
    
    void Start()
    {
        _isGameManagerNotNull = GameManager.Instance != null;
        _crosshairRectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (_isGameManagerNotNull)
        {
            UpdateSize(GameManager.Instance.playerData.weaponSpread);
        }
    }

    public void UpdateSize(float aValue)
    {
        float normal = Mathf.InverseLerp(0f, .3f, aValue);
        float bValue = Mathf.Lerp(55f, 600f, normal);
        _crosshairRectTransform.sizeDelta = new Vector2(bValue, bValue);
    }
}
