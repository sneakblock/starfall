using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBar : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _width;
    private float _height;
    private Rect _rect;
    private Vector2 _origSizeDelta;
    private GameObject _parentGameObject;
    private Image _reloadBarImg;
    private TextMeshProUGUI _reloadBarText;

    public float sizeDeltaX;
    public float sizeDeltaY;

    private void Start()
    {
        _parentGameObject = transform.parent.gameObject;
        _reloadBarImg = _parentGameObject.GetComponentInChildren<Image>();
        _reloadBarText = _parentGameObject.GetComponentInChildren<TextMeshProUGUI>();
        _rectTransform = GetComponentInChildren<RectTransform>();
        _rect = _rectTransform.rect;
        _height = _rect.height;
        _width = _rect.width;
        _origSizeDelta = new Vector2(_width - _width, _height);
        HideReloadBar();
    }

    private void OnEnable()
    {
        RangedWeapon.OnPlayerReload += AnimateReloadBar;
    }

    private void OnDisable()
    {
        RangedWeapon.OnPlayerReload -= AnimateReloadBar;
    }

    private void AnimateReloadBar(float seconds)
    {
        ShowReloadBar();
        _rectTransform.sizeDelta = _origSizeDelta;
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
            yield return null;
        }
        HideReloadBar();
    }

    void ShowReloadBar()
    {
        if (_reloadBarImg)
        {
            _reloadBarImg.enabled = true;
        }

        if (_reloadBarText)
        {
            _reloadBarText.enabled = true;
        }
    }

    void HideReloadBar()
    {
        if (_reloadBarImg)
        {
            _reloadBarImg.enabled = false;
        }

        if (_reloadBarText)
        {
            _reloadBarText.enabled = false;
        }
    }
}
