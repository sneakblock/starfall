using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateScoreMultiplierUI : MonoBehaviour
{
    
    private TextMeshProUGUI multText;

    void Awake()
    {
        Score.OnUpdateMultiplier += updateMult;
        multText = GetComponent<TextMeshProUGUI>();
        Score.setupScore();
    }

    public void updateMult(int newMult)
    {
        multText.text = newMult + "X";
    }
}
