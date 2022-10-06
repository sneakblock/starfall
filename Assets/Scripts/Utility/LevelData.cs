using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public struct Enemies
    {
        [Tooltip(
        "An enemy type found in the level.")]
        public GameObject enemyType;
        [Tooltip(
        "The max amount of times this enemy type can respawn in this level.")]
        public int enemyNumRespawns;
    }

    [Tooltip(
        "An array of enemy types and their number of respawns for the current level.")]
    public Enemies[] enemies;
}
