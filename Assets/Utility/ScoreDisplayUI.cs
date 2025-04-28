using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using Reown.AppKit.Unity; // ou le namespace où se trouve votre WalletManager

public class ScoreDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI personalizedScoreText;
    public float refreshInterval = 10f;
    public string projectID = "pokenads-c58e5";

    private const string TEMP_SCORE_KEY      = "TempNadsScore";
    private const string FIRESTORE_SCORE_KEY = "FirestoreScore";
    private const string DISPLAY_TEXT_KEY    = "CurrentNadsDisplay";

    private int lastKnownFirestoreScore = 0;

    void Awake()
    {
        if (PlayerPrefs.HasKey(TEMP_SCORE_KEY) && personalizedScoreText != null)
        {
            int tempScore = PlayerPrefs.GetInt(TEMP_SCORE_KEY);
            int baseScore = PlayerPrefs.GetInt(FIRESTORE_SCORE_KEY, 0);
            personalizedScoreText.text = "Nads: " + (baseScore + tempScore);
            PlayerPrefs.SetString(DISPLAY_TEXT_KEY, personalizedScoreText.text);
            PlayerPrefs.Save();
        }
        else if (PlayerPrefs.HasKey(DISPLAY_TEXT_KEY) && personalizedScoreText != null)
        {
            personalizedScoreText.text = PlayerPrefs.GetString(DISPLAY_TEXT_KEY);
        }
    }

    void OnEnable()
    {
        WalletManager.OnWalletAddressChanged += HandleWalletChanged;
    }

    void OnDisable()
    {
        WalletManager.OnWalletAddressChanged -= HandleWalletChanged;
    }

    void Start()
    {
        // initialisation & premier refresh
        if (!PlayerPrefs.HasKey(DISPLAY_TEXT_KEY) && personalizedScoreText != null)
        {
            personalizedScoreText.text = "Nads: loading";
            int total = PlayerPrefs.GetInt(TEMP_SCORE_KEY, 0) + PlayerPrefs.GetInt(FIRESTORE_SCORE_KEY, 0);
            if (total > 0)
            {
                personalizedScoreText.text = "Nads: " + total;
                PlayerPrefs.SetString(DISPLAY_TEXT_KEY, personalizedScoreText.text);
                PlayerPrefs.Save();
            }
            else
            {
                Invoke("RefreshScore", 1f);
            }
        }

        InvokeRepeating("RefreshScore", refreshInterval, refreshInterval);

        // NOUVEAU : si on est déjà déconnecté, on arrête tout et remise à zéro
        if (string.IsNullOrEmpty(WalletManager.CurrentWalletAddress))
        {
            CancelInvoke("RefreshScore");
            if (personalizedScoreText != null)
                personalizedScoreText.text = "Nads: 0";
        }
    }

    private void HandleWalletChanged(string walletAddress)
    {
        if (string.IsNullOrEmpty(walletAddress))
        {
            // déconnecté
            CancelInvoke("RefreshScore");
            if (personalizedScoreText != null)
                personalizedScoreText.text = "Nads: 0";
        }
        else
        {
            // reconnecté
            RefreshScore();
            InvokeRepeating("RefreshScore", refreshInterval, refreshInterval);
        }
    }

    public void RefreshScore()
    {
        var addr = WalletManager.CurrentWalletAddress;
        if (string.IsNullOrEmpty(addr))
            return;

        string url = $"https://firestore.googleapis.com/v1/projects/{projectID}/databases/(default)/documents/Scores/{addr}";
        StartCoroutine(GetScoreCoroutine(url));
    }

    IEnumerator GetScoreCoroutine(string url)
    {
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            // en cas d’erreur réseau, on ne change rien (ou on garde loading → 0)
            if (personalizedScoreText != null && personalizedScoreText.text == "Nads: loading")
                personalizedScoreText.text = "Nads: 0";
        }
        else
        {
            string json = request.downloadHandler.text;
            int fsScore = 0;
            bool found = false;

            int idx = json.IndexOf("\"Score\":{\"integerValue\":\"");
            if (idx > 0)
            {
                idx += "\"Score\":{\"integerValue\":\"".Length;
                int end = json.IndexOf("\"", idx);
                if (end > idx && int.TryParse(json.Substring(idx, end - idx), out fsScore))
                    found = true;
            }

            if (!found)
            {
                try
                {
                    var doc = JsonUtility.FromJson<FirestoreDocument>(json);
                    if (doc?.fields?.Score != null &&
                        int.TryParse(doc.fields.Score.integerValue, out fsScore))
                        found = true;
                }
                catch { }
            }

            if (found)
            {
                int old = lastKnownFirestoreScore;
                lastKnownFirestoreScore = fsScore;
                PlayerPrefs.SetInt(FIRESTORE_SCORE_KEY, fsScore);

                int sessionPts = PlayerPrefs.GetInt(TEMP_SCORE_KEY, 0);
                if (fsScore >= old + sessionPts)
                    PlayerPrefs.SetInt(TEMP_SCORE_KEY, 0);

                int total = fsScore + PlayerPrefs.GetInt(TEMP_SCORE_KEY, 0);
                if (personalizedScoreText != null)
                {
                    var disp = "Nads: " + total;
                    personalizedScoreText.text = disp;
                    PlayerPrefs.SetString(DISPLAY_TEXT_KEY, disp);
                }
                PlayerPrefs.Save();
            }
            else if (personalizedScoreText != null && personalizedScoreText.text == "Nads: loading")
            {
                personalizedScoreText.text = "Nads: 0";
                PlayerPrefs.SetString(DISPLAY_TEXT_KEY, personalizedScoreText.text);
                PlayerPrefs.Save();
            }
        }
    }
}

// JSON helper classes
[System.Serializable]
public class FirestoreDocument { public FirestoreFields fields; }
[System.Serializable]
public class FirestoreFields { public FirestoreIntegerValue Score; }
[System.Serializable]
public class FirestoreIntegerValue { public string integerValue; }