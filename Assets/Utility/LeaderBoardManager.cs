using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject leaderboardPanel;
    public Transform leaderboardContent;
    public GameObject leaderboardItemPrefab;

    public string leaderboardUrl = "https://votre-backend-api.com/leaderboard";

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerAddress;
        public int score;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries;
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
        StartCoroutine(FetchLeaderboardData());
    }

    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    IEnumerator FetchLeaderboardData()
    {
        UnityWebRequest request = UnityWebRequest.Get(leaderboardUrl);
        yield return request.SendWebRequest();

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors du chargement du leaderboard : " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(jsonResponse);
            if(data != null && data.entries != null)
            {
                PopulateLeaderboard(data.entries);
            }
            else
            {
                Debug.LogError("Erreur de parsing du leaderboard");
            }
        }
    }

    void PopulateLeaderboard(List<LeaderboardEntry> entries)
    {
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        foreach (LeaderboardEntry entry in entries)
        {
            GameObject item = Instantiate(leaderboardItemPrefab, leaderboardContent);
            Text[] texts = item.GetComponentsInChildren<Text>();
            if(texts.Length >= 2)
            {
                texts[0].text = entry.playerAddress;
                texts[1].text = entry.score.ToString();
            }
        }
    }
}
