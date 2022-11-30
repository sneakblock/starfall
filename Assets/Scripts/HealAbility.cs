using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class HealAbility : AdvancedAbility
{
    [SerializeField] private VisualEffect effect;
    [SerializeField] public float healRadius = 5f;
    [SerializeField] private float healAmount = 20f;

    public override void OnCastStarted()
    {
        base.OnCastStarted();
        if (effect) effect.Play();
        foreach (var enemyObject in AIManager.Instance.activeEnemies.Where(enemyObject => Vector3.Distance(gameObject.transform.position, enemyObject.transform.position) <= healRadius))
        {
            enemyObject.GetComponent<SAi>().Heal(healAmount);
        }
    }
}
