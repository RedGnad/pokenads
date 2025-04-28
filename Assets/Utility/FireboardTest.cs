using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class FirebaseTest : MonoBehaviour
{
    public static FirebaseTest Instance { get; private set; }
    public bool IsScoreUpdateInProgress { get; private set; }

    private string commitUrl = "https://firestore.googleapis.com/v1/projects/pokenads-c58e5/databases/(default)/documents:commit";
    private const string WALLET_ADDRESS_KEY = "LastConnectedWallet";
    private const string TEMP_SCORE_KEY = "TempNadsScore";
    private const string UPDATE_PROCESSED_KEY = "ScoreUpdateAlreadyProcessed";
    
    [SerializeField] private int scoreIncrement = 20;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Réinitialiser le flag si on n'est pas sur la scène principale
        if (scene.buildIndex != 0)
        {
            PlayerPrefs.DeleteKey(UPDATE_PROCESSED_KEY);
            PlayerPrefs.Save();
            return;
        }

        // Traitement uniquement pour la scène principale (index 0)
        if (PlayerPrefs.HasKey(TEMP_SCORE_KEY) && PlayerPrefs.GetInt(UPDATE_PROCESSED_KEY, 0) != 1)
        {
            Debug.Log("Détection d'un score temporaire à mettre à jour");
            TestPatchEntry();
            
            // Marquer comme traité immédiatement pour éviter le double traitement
            PlayerPrefs.SetInt(UPDATE_PROCESSED_KEY, 1);
            PlayerPrefs.Save();
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Start()
    {
        SaveWalletAddressIfAvailable();
    }
    
    public void TestPatchEntry()
    {
        string walletAddress = GetWalletAddress();
        
        if (string.IsNullOrEmpty(walletAddress))
        {
            Debug.LogWarning("Wallet non connecté, requête non envoyée.");
            return;
        }

        if (PlayerPrefs.GetInt(UPDATE_PROCESSED_KEY, 0) == 1)
        {
            Debug.Log("Une mise à jour de score a déjà été effectuée, requête ignorée");
            return;
        }

        IsScoreUpdateInProgress = true;
        StartCoroutine(PatchEntryCoroutine(walletAddress));
    }
    
    private string GetWalletAddress()
    {
        string address = WalletManager.CurrentWalletAddress;
        
        if (string.IsNullOrEmpty(address))
        {
            address = PlayerPrefs.GetString(WALLET_ADDRESS_KEY, "");
        }
        else
        {
            SaveWalletAddress(address);
        }
        
        return address;
    }
    
    private void SaveWalletAddress(string address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            PlayerPrefs.SetString(WALLET_ADDRESS_KEY, address);
            PlayerPrefs.Save();
        }
    }
    
    private void SaveWalletAddressIfAvailable()
    {
        string address = WalletManager.CurrentWalletAddress;
        if (!string.IsNullOrEmpty(address))
        {
            SaveWalletAddress(address);
        }
    }

    IEnumerator PatchEntryCoroutine(string walletAddress)
    {
        string documentName = "projects/pokenads-c58e5/databases/(default)/documents/Scores/" + walletAddress;

        string jsonPayload =
            "{" +
            "  \"writes\": [" +
            "    {" +
            "      \"update\": {" +
            "        \"name\": \"" + documentName + "\"," +
            "        \"fields\": {" +
            "          \"User\": { \"stringValue\": \"" + walletAddress + "\" }" +
            "        }" +
            "      }," +
            "      \"updateMask\": { \"fieldPaths\": [\"User\"] }" +
            "    }," +
            "    {" +
            "      \"transform\": {" +
            "        \"document\": \"" + documentName + "\"," +
            "        \"fieldTransforms\": [" +
            "          {" +
            "            \"fieldPath\": \"Score\"," +
            "            \"increment\": { \"integerValue\": \"" + scoreIncrement + "\" }" +
            "          }" +
            "        ]" +
            "      }" +
            "    }" +
            "  ]" +
            "}";

        UnityWebRequest request = new UnityWebRequest(commitUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Score mis à jour avec succès");
            // Nettoyer après mise à jour réussie
            PlayerPrefs.DeleteKey(TEMP_SCORE_KEY);
            PlayerPrefs.DeleteKey("LastScoreTimestamp");
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Erreur lors de la mise à jour du score: " + request.error);
            // En cas d'erreur, autoriser une nouvelle tentative plus tard
            PlayerPrefs.DeleteKey(UPDATE_PROCESSED_KEY);
            PlayerPrefs.Save();
        }

        IsScoreUpdateInProgress = false;
    }
}