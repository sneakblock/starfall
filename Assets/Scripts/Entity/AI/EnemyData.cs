using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{

    [Header("Weapons")] 
    public GameObject weapon;

    [Header("Engagement & Aggression")]
    [Tooltip(
        "These ranges dictate the sweet spot for enemy combat behaviors. An enemy outside of this range will seek to enter it.")]
    public float minEngagementRange;
    public float maxEngagementRange;

    [Tooltip(
        "A normalized value that represents the enemy's accuracy. 0 means they are likely to miss, 1 means that they will always aim at the correct spot.")]
    [Range(0f, 1f)]
    public float accuracy;

}
