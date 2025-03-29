using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using ChainSafe.Gaming.UnityPackage;

public class FirebaseTest : MonoBehaviour
{
    private string commitUrl = "https://firestore.googleapis.com/v1/projects/pokenads-c58e5/databases/(default)/documents:commit";
    
    private string walletAddress = "";

    public void TestPatchEntry()
    {
        if (Web3Unity.Instance != null)
        {
            walletAddress = Web3Unity.Instance.PublicAddress;
        }
        
        if (string.IsNullOrEmpty(walletAddress))
        {
            Debug.LogWarning("Wallet non connecté, requête non envoyée.");
            return;
        }
        
        StartCoroutine(PatchEntryCoroutine());
    }

    IEnumerator PatchEntryCoroutine()
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
            "            \"increment\": { \"integerValue\": \"20\" }" +
            "          }" +
            "        ]" +
            "      }" +
            "    }" +
            "  ]" +
            "}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

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
