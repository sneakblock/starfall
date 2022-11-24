using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    
    [SerializeField]
    private float health;

    private TextMeshPro _tm;

    void Start()
    {
        _tm = GetComponentInChildren<TextMeshPro>();
        _tm.text = health.ToString();
    }

    void Update()
    {
        _tm.text = health.ToString();
    }

    public void Damage(float damage)
    {
        health -= damage;
        if (health <= 0) Kill();
    }

    public void Heal(float healing)
    {
        health += healing;
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public void StartBleeding(float totalDamage, float duration)
    {
        return;
    }
}
