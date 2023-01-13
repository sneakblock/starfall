using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MultiplierLevel
{
    OneEx,
    TwoEx,
    FourEx,
    SixEx,
    EightEx
}

public class Score : MonoBehaviour
{
    [SerializeField]
    private double scoreMultiplier;

    private bool dmgMultDecrEn = true;
    
    // // How long the multiplier increase from getting a kill should last (steadily decreases in 5 increments)
    // private float killMultBuffDuration = 20.0f;
    // // How large the multiplier increase from a kill should be
    // private double killMultInc = 1.0;
    // // How long the cooldown is between the multiplier decreasing from taking damage
    // private float damageMultDecrCooldown = 3.0f;
    // // How much the multiplier decreases from being damaged
    // private double dmgMultDecr = 0.1;
    [SerializeField] private float secondsToMultikill = 3f;
    [SerializeField] private float multiplierDecayRate = .01f;

    [Header("Multiplier Sounds")] public AudioSource scoreMultiplierSource;
    public AudioClip oneExClip;
    public AudioClip twoExClip;
    public AudioClip fourExClip;
    public AudioClip sixExClip;
    public AudioClip eightExClip;

    public MultiplierLevel TrueMultiplierLevel = MultiplierLevel.OneEx;
    private Dictionary<MultiplierLevel, (int, AudioClip)> _levelsInts;

    public static Score Instance;

    public static double savedScore;

    private float lastKillTimestamp;
    private int killChainLength = 0;
    
    //Event for when a new ScoreCounter object is created
    public static event Action<int> OnUpdateScore;
    public static event Action<int> OnUpdateMultiplier;

    void Awake()
    {
        scoreMultiplier = 1;
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }
        OnUpdateScore?.Invoke((int) GameManager.Instance.SessionData.sessionScore);
        OnUpdateMultiplier?.Invoke(1);
        _levelsInts = new Dictionary<MultiplierLevel, (int, AudioClip)>
        {
            {MultiplierLevel.OneEx, (1, oneExClip)},
            {MultiplierLevel.TwoEx, (2, twoExClip)},
            {MultiplierLevel.FourEx, (4, fourExClip)},
            {MultiplierLevel.SixEx, (6, sixExClip)},
            {MultiplierLevel.EightEx, (8, eightExClip)}
        };
    }

    private void OnEnable()
    {
        SAi.OnAIDeath += getKill;
        APlayer.OnPlayerDeath += resetScore;
        // APlayer.OnDamage += damageMultiplierDecr;
    }

    private void OnDisable()
    {
        SAi.OnAIDeath -= getKill;
        APlayer.OnPlayerDeath -= resetScore;
        // APlayer.OnDamage -= damageMultiplierDecr;
    }

    private void Update()
    {
        TrueMultiplierLevel = CalculateTrueMultiplierLevel(scoreMultiplier);
        Debug.Log("backendScore is " + scoreMultiplier);
        Debug.Log("True multiplier is " + TrueMultiplierLevel);
        if (scoreMultiplier > 1) scoreMultiplier -= multiplierDecayRate * _levelsInts[TrueMultiplierLevel].Item1 * Time.deltaTime;
    }

    private MultiplierLevel CalculateTrueMultiplierLevel(double backendScoreMultiplier)
    {
        var newLevel = backendScoreMultiplier switch
        {
            < 2 => MultiplierLevel.OneEx,
            >= 2 and < 4 => MultiplierLevel.TwoEx,
            >= 4 and < 6 => MultiplierLevel.FourEx,
            >= 6 and < 8 => MultiplierLevel.SixEx,
            _ => MultiplierLevel.EightEx
        };
        if (_levelsInts[newLevel].Item1 < _levelsInts[TrueMultiplierLevel].Item1)
        {
            //We have dropped a level.
            scoreMultiplier = _levelsInts[newLevel].Item1 + .25f;
        }
        if (newLevel != TrueMultiplierLevel && newLevel != MultiplierLevel.OneEx && scoreMultiplierSource)
        {
            scoreMultiplierSource.PlayOneShot(_levelsInts[newLevel].Item2, 1f);
        }
        OnUpdateMultiplier?.Invoke(_levelsInts[newLevel].Item1);
        return newLevel;
    }

    private void addToScore(double scoreChange)
    {
        GameManager.Instance.SessionData.sessionScore += scoreChange;
        OnUpdateScore?.Invoke((int) GameManager.Instance.SessionData.sessionScore);
    }

    private void getKill(SAi killedSAi)
    {
        addToScore(killedSAi.linkValue * _levelsInts[TrueMultiplierLevel].Item1);
        IncrementMultiplierOnKill(killedSAi.linkValue);
        TrueMultiplierLevel = CalculateTrueMultiplierLevel(scoreMultiplier);
    }

    void IncrementMultiplierOnKill(float linkValue)
    {
        if (Time.time - lastKillTimestamp <= secondsToMultikill)
        {
            //This was a valid multikill
            killChainLength++;
        }
        else
        {
            killChainLength = 1;
        }

        //A tank gives a full multiplier level.
        var baseMult = Mathf.Lerp(0, 1, linkValue / 100);
        scoreMultiplier += baseMult * killChainLength;
        lastKillTimestamp = Time.time;
    }

    // IEnumerator shrinkMultiplier() 
    // {
    //     for (double i = 5; i > 0; i -= 1) {
    //         yield return new WaitForSeconds(killMultBuffDuration / 5);
    //         decrMultiplier(killMultInc / 5);
    //     }
    // }
    //
    // private void decrMultiplier(double decrease)
    // {
    //     scoreMultiplier -= decrease;
    //     if (scoreMultiplier < 1) scoreMultiplier = 1;
    //     TrueMultiplierLevel = CalculateTrueMultiplierLevel(scoreMultiplier);
    // }

    // private void damageMultiplierDecr()
    // {
    //     if (dmgMultDecrEn && scoreMultiplier > 1) {
    //         decrMultiplier(dmgMultDecr);
    //         StartCoroutine(disableMultDecr());
    //     }
    // }
    //
    // IEnumerator disableMultDecr() 
    // {
    //         dmgMultDecrEn = false;
    //         yield return new WaitForSeconds(damageMultDecrCooldown);
    //         dmgMultDecrEn = true;
    // }

    private void resetScore()
    {
        savedScore = GameManager.Instance.SessionData.sessionScore;
        //GameManager.Instance.SessionData.sessionScore = 0;
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
