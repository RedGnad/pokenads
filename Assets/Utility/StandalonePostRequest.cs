using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using ChainSafe.Gaming.UnityPackage; // Assurez-vous que ce using fonctionne ici

public class StandalonePostRequest : MonoBehaviour
{
    public string relayerUrl = "https://relay-943qhmvwm-redgnads-projects.vercel.app/api/relayInteraction";

    private string walletAddress = "";

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ProcessInput(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
    }

    void ProcessInput(Vector3 screenPos)
    {
        if (Camera.main == null)
        {
            Debug.LogError("MainCamera introuvable ! Assignez le tag 'MainCamera' à votre caméra principale.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                Debug.Log("Tap détecté sur " + gameObject.name);

                if (Web3Unity.Instance == null)
                {
                    Debug.LogError("Web3Unity.Instance est null ! Vérifiez son initialisation dans la scène.");
                    return;
                }

                walletAddress = Web3Unity.Instance.PublicAddress;
                Debug.Log("Wallet address: " + walletAddress);
                
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
        InteractionPayload payload = new InteractionPayload
        {
            playerAddress = walletAddress,
            action = "click"
        };

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