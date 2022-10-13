using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [SerializeField]
    private double score;
    [SerializeField]
    public double killPoints = 10;
    [SerializeField]
    private double scoreMultiplier;

    public static Score Instance;

    public static event Action<int> OnUpdateScore;

    void Awake()
    {
        scoreMultiplier = 1;
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        OnUpdateScore?.Invoke((int) score);
    }

    private void OnEnable()
    {
        SAi.OnAIDeath += getKill;
        APlayer.OnPlayerDeath += resetScore;
        APlayer.OnDamage += decrMultiplier;
    }

    private void OnDisable()
    {
        SAi.OnAIDeath -= getKill;
        APlayer.OnPlayerDeath -= resetScore;
        APlayer.OnDamage -= decrMultiplier;
    }

    private void addToScore(double scoreChange)
    {
        score += scoreChange;
        OnUpdateScore?.Invoke((int) score);
    }

    private void getKill() {
        addToScore(killPoints * scoreMultiplier);
        scoreMultiplier += 1;
        StartCoroutine(shrinkMultiplier());
    }

    IEnumerator shrinkMultiplier() {
        for (double i = 1; i > 0; i -= 0.2) {
            yield return new WaitForSeconds(2.0f);
            decrMultiplier(0.2);
        }
    }

    private void decrMultiplier(double decrease) {
        scoreMultiplier -= decrease;
        if (scoreMultiplier < 1) scoreMultiplier = 1;
    }

    private void resetScore()
    {
        score = 0;
        OnUpdateScore?.Invoke((int) score);
    }

    private static void setupScore()
    {
        Instance.StopAllCoroutines();
        OnUpdateScore?.Invoke((int) Instance.score);
        Instance.scoreMultiplier = 1;
    }
}
