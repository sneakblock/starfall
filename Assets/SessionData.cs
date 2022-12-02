using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SessionData")]
public class SessionData : ScriptableObject
{
    
    public float sessionTotalTime;
    public float secondsToMaxDifficulty = 500;

    public double sessionScore;
    
    public List<string> traversedStageNames;
    
}
