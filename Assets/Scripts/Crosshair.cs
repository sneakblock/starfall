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
    
    void Start()
    {
        _crosshairRectTransform = GetComponent<RectTransform>();
    }

    public void UpdateSize(float aValue)
    {
        float normal = Mathf.InverseLerp(0f, .3f, aValue);
        float bValue = Mathf.Lerp(55f, 600f, normal);
        _crosshairRectTransform.sizeDelta = new Vector2(bValue, bValue);
    }
}
