using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using ChainSafe.Gaming.UnityPackage; // Pour accéder à Web3Unity

public class FirebaseTest : MonoBehaviour
{
    // URL de l'endpoint commit pour Firestore
    private string commitUrl = "https://firestore.googleapis.com/v1/projects/pokenads-c58e5/databases/(default)/documents:commit";
    
    // L'adresse du wallet, récupérée dynamiquement
    private string walletAddress = "";

    // Méthode pour lancer le patch
    public void TestPatchEntry()
    {
        // Récupérer l'adresse du wallet via Web3Unity
        if (Web3Unity.Instance != null)
        {
            walletAddress = Web3Unity.Instance.PublicAddress;
        }
        
        // Si le walletAddress est vide, on ne procède pas à la requête
        if (string.IsNullOrEmpty(walletAddress))
        {
            Debug.LogWarning("Wallet non connecté, requête non envoyée.");
            return;
        }
        
        StartCoroutine(PatchEntryCoroutine());
    }

    IEnumerator PatchEntryCoroutine()
    {
        // Utiliser walletAddress comme identifiant du document dans la collection "Scores"
        string documentName = "projects/pokenads-c58e5/databases/(default)/documents/Scores/" + walletAddress;

        // Préparation du payload JSON pour patcher le document :
        // - Mettre à jour le champ "User" avec walletAddress.
        // - Incrémenter le champ "Score" de 20.
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
            "            \"increment\": { \"integerValue\": \"20\" }" +
            "          }" +
            "        ]" +
            "      }" +
            "    }" +
            "  ]" +
            "}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        // Affichage des logs pour diagnostic
        Debug.Log("Payload JSON pour patch: " + jsonPayload);
        Debug.Log("Envoi de la requête PATCH vers: " + commitUrl);

        UnityWebRequest request = new UnityWebRequest(commitUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Réponse patch Firestore : " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Erreur lors du patch Firestore : " + request.error);
        }
    }
}
