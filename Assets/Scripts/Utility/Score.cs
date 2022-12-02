using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [SerializeField]
    private double scoreMultiplier;

    private bool dmgMultDecrEn = true;
    
    // How long the multiplier increase from getting a kill should last (steadily decreases in 5 increments)
    private float killMultBuffDuration = 20.0f;
    // How large the multiplier increase from a kill should be
    private double killMultInc = 1.0;
    // How long the cooldown is between the multiplier decreasing from taking damage
    private float damageMultDecrCooldown = 3.0f;
    // How much the multiplier decreases from being damaged
    private double dmgMultDecr = 0.1;

    public static Score Instance;

    public static double savedScore;

    //Event for when a new ScoreCounter object is created
    public static event Action<int> OnUpdateScore;

    void Awake()
    {
        scoreMultiplier = 1;
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        OnUpdateScore?.Invoke((int) GameManager.Instance.SessionData.sessionScore);
    }

    private void OnEnable()
    {
        SAi.OnAIDeath += getKill;
        APlayer.OnPlayerDeath += resetScore;
        APlayer.OnDamage += damageMultiplierDecr;
    }

    private void OnDisable()
    {
        SAi.OnAIDeath -= getKill;
        APlayer.OnPlayerDeath -= resetScore;
        APlayer.OnDamage -= damageMultiplierDecr;
    }

    private void addToScore(double scoreChange)
    {
        GameManager.Instance.SessionData.sessionScore += scoreChange;
        OnUpdateScore?.Invoke((int) GameManager.Instance.SessionData.sessionScore);
    }

    private void getKill(SAi killedSAi)
    {
        addToScore(killedSAi.linkValue * scoreMultiplier);
        scoreMultiplier += killMultInc;
        StartCoroutine(shrinkMultiplier());
    }

    IEnumerator shrinkMultiplier() 
    {
        for (double i = 5; i > 0; i -= 1) {
            yield return new WaitForSeconds(killMultBuffDuration / 5);
            decrMultiplier(killMultInc / 5);
        }
    }

    private void decrMultiplier(double decrease)
    {
        scoreMultiplier -= decrease;
        if (scoreMultiplier < 1) scoreMultiplier = 1;
    }

    private void damageMultiplierDecr()
    {
        if (dmgMultDecrEn && scoreMultiplier > 1) {
            decrMultiplier(dmgMultDecr);
            StartCoroutine(disableMultDecr());
        }
    }

    IEnumerator disableMultDecr() 
    {
            dmgMultDecrEn = false;
            yield return new WaitForSeconds(damageMultDecrCooldown);
            dmgMultDecrEn = true;
    }

    private void resetScore()
    {
        savedScore = GameManager.Instance.SessionData.sessionScore;
        GameManager.Instance.SessionData.sessionScore = 0;
        OnUpdateScore?.Invoke((int) GameManager.Instance.SessionData.sessionScore);
    }

    public static void setupScore()
    {
        //TODO: NULL REFS.
        Instance.StopAllCoroutines();
        OnUpdateScore?.Invoke((int) GameManager.Instance.SessionData.sessionScore);
        Instance.scoreMultiplier = 1;
    }

    public static double getSavedScore(){
        return savedScore;
    }
}
