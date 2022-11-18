using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailFade : MonoBehaviour
{
    [SerializeField] private float trailFadeSpeed = 10f;
    [SerializeField] public Color color;

    private LineRenderer _lr;
    // Start is called before the first frame update
    void Start()
    {
        _lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_lr) return;
        color.a = Mathf.Lerp(color.a, 0, trailFadeSpeed * Time.deltaTime);
        _lr.startColor = color;
        _lr.endColor = color;
        if (color.a <= .1f) GameManager.Instance.BulletTrailPool.Release(gameObject);
    }
}
