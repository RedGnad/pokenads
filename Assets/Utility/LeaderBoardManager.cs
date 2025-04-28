using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
// Supprimé: using ChainSafe.Gaming.UnityPackage;

public class LeaderBoardManager : MonoBehaviour
{
    [SerializeField] private RectTransform leaderboardContent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    
    [SerializeField] private string projectID = "pokenads-c58e5";
    
    private void Start()
    {
        RefreshLeaderboard();
    }
    
    public void RefreshLeaderboard()
    {
        StartCoroutine(GetLeaderboardData());
    }
    
    private IEnumerator GetLeaderboardData()
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectID}/databases/(default)/documents/Scores?orderBy=fields.Score.integerValue%20desc&pageSize=10";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erreur lors de la récupération du leaderboard: " + request.error);
            }
            else
            {
                ClearLeaderboard();
                string jsonResponse = request.downloadHandler.text;
                LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(jsonResponse);
                
                if (response != null && response.documents != null)
                {
                    int rank = 1;
                    foreach (var doc in response.documents)
                    {
                        string address = GetAddressFromDocumentName(doc.name);
                        int score = int.Parse(doc.fields.Score.integerValue);
                        
                        AddLeaderboardEntry(rank, address, score);
                        rank++;
                    }
                    
                    string playerAddress = ConnectWalletButton.CurrentWalletAddress;
                    if (!string.IsNullOrEmpty(playerAddress))
                    {
                        StartCoroutine(GetPlayerScore(playerAddress));
                    }
                    else
                    {
                        if (playerScoreText != null)
                        {
                            playerScoreText.text = "Connectez votre wallet pour voir votre score";
                        }
                    }
                }
            }
        }
    }
    
    private IEnumerator GetPlayerScore(string address)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectID}/databases/(default)/documents/Scores/{address}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                LeaderboardDocument doc = JsonUtility.FromJson<LeaderboardDocument>(jsonResponse);
                
                if (doc != null && doc.fields != null && doc.fields.Score != null)
                {
                    int score = int.Parse(doc.fields.Score.integerValue);
                    if (playerScoreText != null)
                    {
                        playerScoreText.text = $"Votre score: {score}";
                    }
                }
            }
            else
            {
                if (playerScoreText != null)
                {
                    playerScoreText.text = "Score non disponible";
                }
                Debug.LogWarning("Erreur lors de la récupération du score du joueur: " + request.error);
            }
        }
    }
    
    // Reste du code inchangé...
    private void ClearLeaderboard()
    {
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void AddLeaderboardEntry(int rank, string address, int score)
    {
        GameObject entry = Instantiate(entryPrefab, leaderboardContent);
        TextMeshProUGUI rankText = entry.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI addressText = entry.transform.Find("AddressText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = entry.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        
        if (rankText != null) rankText.text = rank.ToString();
        if (addressText != null) addressText.text = FormatAddress(address);
        if (scoreText != null) scoreText.text = score.ToString();
    }
    
    private string GetAddressFromDocumentName(string documentName)
    {
        // Format: projects/pokenads-c58e5/databases/(default)/documents/Scores/0x123...
        string[] parts = documentName.Split('/');
        return parts[parts.Length - 1];
    }
    
    private string FormatAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length <= 10) 
            return address;
        
        return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
    }
}

[System.Serializable]
public class LeaderboardResponse
{
    public LeaderboardDocument[] documents;
}

[System.Serializable]
public class LeaderboardDocument
{
    public string name;
    public LeaderboardFields fields;
}

[System.Serializable]
public class LeaderboardFields
{
    public LeaderboardIntegerValue Score;
}

[System.Serializable]
public class LeaderboardIntegerValue
{
    public string integerValue;
}