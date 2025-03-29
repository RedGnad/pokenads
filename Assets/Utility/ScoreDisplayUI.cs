using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using ChainSafe.Gaming.UnityPackage;

public class ScoreDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI personalizedScoreText;
    public float refreshInterval = 10f;
    public string projectID = "pokenads-c58e5";

    void Start()
    {
        if (personalizedScoreText != null)
            personalizedScoreText.text = "Nads: loading";

        StartCoroutine(DelayedRefreshScore());

        InvokeRepeating(nameof(RefreshScore), refreshInterval + 3f, refreshInterval);
    }

    IEnumerator DelayedRefreshScore()
    {
        yield return new WaitForSeconds(3f);
        RefreshScore();
    }

    public void RefreshScore()
    {
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

            FirestoreDocument doc = JsonUtility.FromJson<FirestoreDocument>(json);
            int score = 0;
            if (doc != null && doc.fields != null && doc.fields.Score != null)
            {
                int.TryParse(doc.fields.Score.integerValue, out score);
            }
            if (personalizedScoreText != null)
            {
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
}

[System.Serializable]
public class FirestoreDocument
{
    public FirestoreFields fields;
}