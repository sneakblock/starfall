using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBar : MonoBehaviour
{
    private RectTransform _rectTransform;
    private GameObject _parentGO;
    private float _width;
    private float _height;
    private Rect _rect;
    private Vector2 _origSizeDelta;

    public float sizeDeltaX;
    public float sizeDeltaY;

    private void Start()
    {
        _rectTransform = GetComponentInChildren<RectTransform>();
        _parentGO = transform.parent.gameObject;
        _rect = _rectTransform.rect;
        _height = _rect.height;
        _width = _rect.width;
        _origSizeDelta = new Vector2(_width - _width, _height);
        _parentGO.SetActive(false);
    }

    public void AnimateReloadBar(float seconds)
    {
        _rectTransform.sizeDelta = _origSizeDelta;
        _parentGO.SetActive(true);
        StartCoroutine(ShrinkBar(_width - _width, -_width, seconds));
    }

    private void Update()
    {
        _rectTransform.sizeDelta = new Vector2(sizeDeltaX, sizeDeltaY);
    }

    IEnumerator ShrinkBar(float widthA, float widthB, float numSeconds)
    {
        for (float t = 0f; t < numSeconds; t += Time.deltaTime)
        {
            _rectTransform.sizeDelta = new Vector2(Mathf.Lerp(widthA, widthB, t / numSeconds), 3f);
            Debug.Log(_rectTransform.sizeDelta);
            yield return null;
        }
        _parentGO.SetActive(false);
    }
}
