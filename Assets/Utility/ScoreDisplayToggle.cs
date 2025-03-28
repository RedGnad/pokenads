using UnityEngine;
using TMPro;
using ChainSafe.Gaming.UnityPackage; // Pour accéder à Web3Unity

public class WalletDisplayToggle : MonoBehaviour
{
    public TextMeshProUGUI connectedTMP;
    public TextMeshProUGUI disconnectedTMP;

    void Start()
    {
        // Vérifie toutes les 1 seconde si le wallet est renseigné
        InvokeRepeating(nameof(ToggleDisplay), 0f, 1f);
    }

    public void ToggleDisplay()
    {
        string walletAddress = "";
        if (Web3Unity.Instance != null)
            walletAddress = Web3Unity.Instance.PublicAddress;

        if (!string.IsNullOrEmpty(walletAddress))
        {
            // Si wallet renseigné : afficher connectedTMP et masquer disconnectedTMP
            if (connectedTMP != null && !connectedTMP.gameObject.activeSelf)
                connectedTMP.gameObject.SetActive(true);
            if (disconnectedTMP != null && disconnectedTMP.gameObject.activeSelf)
                disconnectedTMP.gameObject.SetActive(false);
        }
        else
        {
            // Sinon, afficher disconnectedTMP et masquer connectedTMP
            if (connectedTMP != null && connectedTMP.gameObject.activeSelf)
                connectedTMP.gameObject.SetActive(false);
            if (disconnectedTMP != null && !disconnectedTMP.gameObject.activeSelf)
                disconnectedTMP.gameObject.SetActive(true);
        }
    }
}