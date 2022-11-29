using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct PlayerScore
{
    public string username;
    public int score;

    public PlayerScore(string username, int score) {
        this.username = username;
        this.score = score;
    }
}