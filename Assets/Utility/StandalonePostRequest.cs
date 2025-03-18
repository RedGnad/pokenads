using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using ChainSafe.Gaming.UnityPackage; // Assurez-vous que ce using fonctionne ici

public class StandalonePostRequest : MonoBehaviour
{
    // URL de votre relayer
    public string relayerUrl = "https://relay-943qhmvwm-redgnads-projects.vercel.app/api/relayInteraction";

    // L'adresse du wallet sera récupérée dynamiquement
    private string walletAddress = "";

    void Update()
    {
        // Pour les appareils tactiles
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ProcessInput(Input.GetTouch(0).position);
        }
        // Pour les entrées souris/desktop
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
    }

    void ProcessInput(Vector3 screenPos)
    {
        // Vérifier que la caméra principale existe
        if (Camera.main == null)
        {
            Debug.LogError("MainCamera introuvable ! Assignez le tag 'MainCamera' à votre caméra principale.");
            return;
        }

        // Créer un ray à partir de la caméra principale
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Vérifier que l'objet touché est bien ce GameObject
            if (hit.transform == transform)
            {
                Debug.Log("Tap détecté sur " + gameObject.name);

                // Vérifier que l'instance Web3Unity existe avant d'y accéder
                if (Web3Unity.Instance == null)
                {
                    Debug.LogError("Web3Unity.Instance est null ! Vérifiez son initialisation dans la scène.");
                    return;
                }

                // Récupérer l'adresse du wallet via Web3Unity
                walletAddress = Web3Unity.Instance.PublicAddress;
                Debug.Log("Wallet address: " + walletAddress);
                
                // Vérifier que l'adresse n'est pas vide
                if (!string.IsNullOrEmpty(walletAddress))
                {
                    StartCoroutine(SendInteraction());
                }
                else
                {
                    Debug.LogWarning("Wallet non connecté");
                }
            }
        }
    }

    IEnumerator SendInteraction()
    {
        // Création du payload avec uniquement les champs attendus par le backend
        InteractionPayload payload = new InteractionPayload
        {
            playerAddress = walletAddress,
            action = "click"
        };

        // Conversion en JSON
        string jsonData = JsonUtility.ToJson(payload);
        Debug.Log("Sending payload: " + jsonData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(relayerUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending interaction: " + request.error);
            }
            else
            {
                Debug.Log("Interaction sent successfully: " + request.downloadHandler.text);
            }
        }
    }
}

[System.Serializable]
public class InteractionPayload
{
    public string playerAddress;
    public string action;
}