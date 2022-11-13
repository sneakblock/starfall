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
            "The number of enemies of this type that initially spawn in this level.")]
        public int enemyNumSpawns;
        [Tooltip(
            "The max number of times this enemy type can respawn in this level.")]
        public int enemyNumRespawns;
        [Tooltip(
            "The amount by which this enemy's max alive count is incremented during each instance of difficulty scaling.")]
        public int enemyIncrementMaxAlive;
    }

    [Tooltip(
        "An array of enemy types and their number of respawns for the current level.")]
    public Enemies[] enemies;
}
