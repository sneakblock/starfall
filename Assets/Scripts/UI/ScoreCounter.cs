using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI scoreText;

    void Awake()
    {
        Score.OnUpdateScore += updateScore;
        scoreText = GetComponentInChildren<TextMeshProUGUI>();
        Score.setupScore();
    }

    public void updateScore(int newScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
    }
}
