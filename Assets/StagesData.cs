using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "StagesData")]
public class StagesData : ScriptableObject
{
    [System.Serializable]
    public struct StageEnemyData
    {
        [Tooltip(
            "An enemy type found in the level.")]
        public GameObject enemyType;

        [Tooltip("The max number used to scale this enemy's stats at max difficulty.")]
        public float maxBuffScale;
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
