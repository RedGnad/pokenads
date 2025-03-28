using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using ChainSafe.Gaming.UnityPackage; // Pour accéder à Web3Unity

public class ScoreDisplayUI : MonoBehaviour
{
    // Élément UI pour afficher le score personnalisé
    public TextMeshProUGUI personalizedScoreText;
    // Délai (en secondes) entre chaque rafraîchissement
    public float refreshInterval = 10f;
    // Identifiant de votre projet Firestore
    public string projectID = "pokenads-c58e5";

    void Start()
    {
        RefreshScore();
        InvokeRepeating(nameof(RefreshScore), refreshInterval, refreshInterval);
    }

    public void RefreshScore()
    {
        // Récupère l'adresse du wallet via Web3Unity, comme dans FireboardTest.cs
        string walletAddress = "";
        if (Web3Unity.Instance != null)
        {
            walletAddress = Web3Unity.Instance.PublicAddress;
        }
        if (string.IsNullOrEmpty(walletAddress))
        {
            if (personalizedScoreText != null)
                personalizedScoreText.text = "Nads: loading";
            return;
        }

        // Construire l'URL pour accéder au document Firestore dans la collection "Scores"
        string url = $"https://firestore.googleapis.com/v1/projects/{projectID}/databases/(default)/documents/Scores/{walletAddress}";
        StartCoroutine(GetScoreCoroutine(url));
    }

    IEnumerator GetScoreCoroutine(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogError("Erreur lors de la récupération du score : " + request.error);
            if (personalizedScoreText != null)
                personalizedScoreText.text = "Nads: loading";
        }
        else
        {
            string json = request.downloadHandler.text;
            Debug.Log("Réponse Firestore : " + json);

            // Structure JSON attendue :
            // {
            //   "fields": {
            //     "Score": { "integerValue": "42" },
            //     "User": { "stringValue": "0xABCD..." }
            //   },
            //   ...
            // }
            FirestoreDocument doc = JsonUtility.FromJson<FirestoreDocument>(json);
            int score = 0;
            if (doc != null && doc.fields != null && doc.fields.Score != null)
            {
                int.TryParse(doc.fields.Score.integerValue, out score);
            }
            if (personalizedScoreText != null)
            {
                // Si le score est inférieur ou égal à 0, affiche "Nads: 0"
                personalizedScoreText.text = "Nads: " + (score > 0 ? score.ToString() : "0");
            }
        }
    }
}

[System.Serializable]
public class FirestoreInteger
{
    public string integerValue;
}

[System.Serializable]
public class FirestoreFields
{
    public FirestoreInteger Score;
    // D'autres champs peuvent être ajoutés si nécessaire
}

[System.Serializable]
public class FirestoreDocument
{
    public FirestoreFields fields;
}