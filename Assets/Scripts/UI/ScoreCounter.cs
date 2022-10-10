using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    [SerializeField]
    private int score;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    void Awake()
    {
        scoreText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        resetScore();
        SAi.OnAIDeath += updateScore;
        APlayer.OnPlayerDeath += resetScore;
    }

    private void OnDisable()
    {
        SAi.OnAIDeath -= updateScore;
        APlayer.OnPlayerDeath -= resetScore;
    }

    public void updateScore(int scoreChange)
    {
        score += scoreChange;
        scoreText.text = score.ToString();
    }

    public void resetScore()
    {
        score = 0;
        scoreText.text = score.ToString();
    }
}
