using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Leaderboards : MonoBehaviour
{

    public readonly string uri = "https://web-production-7cdc.up.railway.app/api/";

    public List<PlayerScore> leaderboards = new List<PlayerScore>();

    private bool jobQueued = false;
    private PlayerScore job;

    void Start()
    {
        StartCoroutine(FetchLeaderboards());
    }

    void Update()
    {
        if (jobQueued)
        {
            StartCoroutine(PushNewScore(job));
            jobQueued = false;
        }
    }

    public void QueueNewScore(PlayerScore job)
    {
        this.job = job;
        jobQueued = true;
    }

    public List<PlayerScore> Top10()
    {
        leaderboards.Sort((p1, p2) => p2.score - p1.score);
        
        int size = leaderboards.Count > 10 ? 10 : leaderboards.Count;

        List<PlayerScore> output = new List<PlayerScore>();
        for (int i = 0; i < size; i++){
            output.Add(leaderboards[i]);
        }

        return output;
    }

    IEnumerator PushNewScore(PlayerScore job)
    {
        string data = "updatescore?username=" + job.username + "&score=" + job.score;
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        UnityWebRequest www = UnityWebRequest.Post(uri + data, formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("LEADERBOARD cannot update score " + www.error);
        }
        else
        {
            Debug.Log("LEADERBOARD: Form upload complete!");
        }
    }

    IEnumerator FetchLeaderboards()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri + "leaderboard"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                foreach (string part in webRequest.downloadHandler.text.Split("},"))
                {
                    int index = part.IndexOf("username");
                    string leftover = part.Substring(index + 11);
                    string username = leftover.Substring(0, leftover.IndexOf(",") - 1);
                    index = part.IndexOf("score");
                    leftover = part.Substring(index + 8);
                    string score = leftover.Substring(0, leftover.IndexOf(",") - 1);
                    PlayerScore player = new PlayerScore(username, int.Parse(score));
                    leaderboards.Add(player);
                }
                Debug.Log("LEADERBOARD: fetch complete");
            }
            else
            {
                Debug.LogError("LEADERBOARD Unable to retrieve leaderboards: " + webRequest.error);
            }

        }
    }


}