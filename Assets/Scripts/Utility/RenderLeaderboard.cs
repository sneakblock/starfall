using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RenderLeaderboard : MonoBehaviour
{
    public TextMeshProUGUI[] entries;

    void Awake() {
        startRenderLeaderboard();
    }

    //Get top 10 scores and set them as entries
    public void renderLeaderboard(List<PlayerScore> newBoard)
    {
        newBoard.Sort((p1, p2) => p2.score - p1.score);
        
        int size = newBoard.Count > 10 ? 10 : newBoard.Count;

        for (int i = 0; i < size; i++){
            entries[i].text = newBoard[i].username + ": " + newBoard[i].score;
        }
    }

    //Recieve leaderboard callback after coroutine finishes, then execute renderLeaderboard
    public void startRenderLeaderboard() {
        Leaderboards leaderboard = gameObject.AddComponent<Leaderboards>();
        StartCoroutine(leaderboard.FetchLeaderboards((status)=>{
            renderLeaderboard(status);
        }));
    }

    //manage delegate function
    private void OnEnable()
    {
        InputScoreController.callExternally += startRenderLeaderboard;
    }

    private void OnDisable()
    {
        InputScoreController.callExternally -= startRenderLeaderboard;
    }
}
