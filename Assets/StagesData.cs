using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "StagesData")]
public class StagesData : ScriptableObject
{
    [System.Serializable]
    public struct PopulationAtDifficulty
    {
        public DifficultyLevel DifficultyLevel;
        public float percentageOfPopulation;
    }
    
    [System.Serializable]
    public struct StageEnemyData
    {
        [FormerlySerializedAs("enemyType")]
        [Tooltip(
            "An enemy type found in the level.")]
        public GameObject enemyObject;

        public EnemyType EnemyType;

        [Tooltip("The max number used to scale this enemy's stats at max difficulty.")]
        public float maxBuffScale;

        public PopulationAtDifficulty[] PopulationAtDifficulties;
    }
    
    [System.Serializable]
    public struct Stage
    {
        public string StageName;
        public StageEnemyData[] StageEnemyDatas;
        public AudioClip[] StageSongs;
    }

    public Stage[] Stages;
}
