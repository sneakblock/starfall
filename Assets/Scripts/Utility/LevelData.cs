using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{ 
    [Header("Enemy Spawning")]
    [Tooltip(
        "The enemy types found in the level.")]
    public List<GameObject> enemyTypes = new List<GameObject>();
    [Tooltip(
        "If the number of alive enemies falls below this threshold, enemies will respawn until this number is reached.")]
    public int maxAliveEnemyCount;
    [Tooltip(
        "Enemies will stop respawning after the number of total respawns passes this threshold.")]
    public int maxEnemyRespawns;
}
