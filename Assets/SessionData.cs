using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "SessionData")]
public class SessionData : ScriptableObject
{
    
    public float sessionTotalTime;
    public float secondsToMaxDifficulty = 500;

    public double sessionScore;
    
    [FormerlySerializedAs("traversedStageNames")] public List<StagesData.Stage> traversedStages;
    
}
