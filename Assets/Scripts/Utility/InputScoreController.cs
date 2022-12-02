using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputScoreController : MonoBehaviour
{
    public TextMeshProUGUI drawScore;
    public GameObject inputField;
    Leaderboards leaderboard;

    public delegate void CallExternally();
    public static CallExternally callExternally;

    private int score;

    void Start()
    {
        score = GameManager.finalScore;
        Debug.Log($"Score: {score}");
        drawScore.text = score + "";
        leaderboard = gameObject.AddComponent<Leaderboards>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            submitValue();
            Destroy(inputField);
            drawScore.text = "Saved.";
        }
    }

    //ensure boolean callback is true, call delegate function after coroutine finish
    private void submitValue() {
        string text = inputField.GetComponent<TMP_InputField>().text;
        PlayerScore newScore = new PlayerScore(text,score);
        StartCoroutine(leaderboard.PushNewScore(newScore, (status)=>{
            if (status) {
                callExternally.Invoke();
            }
        }));
    }

}
