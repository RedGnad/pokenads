using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Elements")]
    // Le Panel qui contient l'écran du leaderboard (à désactiver par défaut)
    public GameObject leaderboardPanel;
    // Le conteneur pour les entrées du leaderboard, par exemple le "Content" d'une ScrollView
    public Transform leaderboardContent;
    // Le prefab représentant une entrée du leaderboard
    public GameObject leaderboardItemPrefab;

    // URL de l'API ou du serveur qui renvoie les données du leaderboard
    public string leaderboardUrl = "https://votre-backend-api.com/leaderboard";

    // Classes pour parser le JSON renvoyé par le serveur
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

    /// <summary>
    /// Affiche l'écran du leaderboard et charge les données.
    /// </summary>
    public void ShowLeaderboard()
    {
        // Active le panel du leaderboard
        leaderboardPanel.SetActive(true);
        // Efface toutes les anciennes entrées
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
        // Lance la récupération des données du leaderboard
        StartCoroutine(FetchLeaderboardData());
    }

    /// <summary>
    /// Cache l'écran du leaderboard.
    /// </summary>
    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    /// <summary>
    /// Récupère les données du leaderboard depuis le serveur.
    /// </summary>
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
            // Récupérer et parser la réponse JSON
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

    /// <summary>
    /// Instancie dynamiquement les entrées du leaderboard dans le conteneur.
    /// </summary>
    void PopulateLeaderboard(List<LeaderboardEntry> entries)
    {
        // Trier les entrées par score décroissant (optionnel)
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        foreach (LeaderboardEntry entry in entries)
        {
            // Instanciation du prefab dans le conteneur (leaderboardContent)
            GameObject item = Instantiate(leaderboardItemPrefab, leaderboardContent);
            // Supposons que le prefab contient deux Texts : le premier pour l'adresse, le second pour le score
            Text[] texts = item.GetComponentsInChildren<Text>();
            if(texts.Length >= 2)
            {
                texts[0].text = entry.playerAddress;
                texts[1].text = entry.score.ToString();
            }
        }
    }
}
