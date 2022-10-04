using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    
    [SerializeField]
    private int health;

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

    public void Damage(int damage)
    {
        health -= damage;
        if (health <= 0) Kill();
    }

    public void Heal(int healing)
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
}
