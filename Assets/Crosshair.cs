using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{

    private RectTransform _crosshairRectTransform;
    private RangedWeapon _playerWeapon;

    [SerializeField]
    [Range(55f, 600f)]
    private float size;
    
    // Start is called before the first frame update
    void Start()
    {
        _crosshairRectTransform = GetComponent<RectTransform>();
        var player = GameObject.FindWithTag("Player");
        _playerWeapon = player.GetComponent<StarfallCharacterController>().GetRangedWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        float aValue = _playerWeapon.GetCurrentSpread();
        float normal = Mathf.InverseLerp(0f, .3f, aValue);
        float bValue = Mathf.Lerp(55f, 600f, normal);
        _crosshairRectTransform.sizeDelta = new Vector2(bValue, bValue);
    }
}
