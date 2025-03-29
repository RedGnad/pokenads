using UnityEngine;
using TMPro;
using ChainSafe.Gaming.UnityPackage; 

public class WalletDisplayToggle : MonoBehaviour
{
    public TextMeshProUGUI connectedTMP;
    public TextMeshProUGUI disconnectedTMP;

    void Start()
    {
        InvokeRepeating(nameof(ToggleDisplay), 0f, 1f);
    }

    public void ToggleDisplay()
    {
        string walletAddress = "";
        if (Web3Unity.Instance != null)
            walletAddress = Web3Unity.Instance.PublicAddress;

        if (!string.IsNullOrEmpty(walletAddress))
        {
            if (connectedTMP != null && !connectedTMP.gameObject.activeSelf)
                connectedTMP.gameObject.SetActive(true);
            if (disconnectedTMP != null && disconnectedTMP.gameObject.activeSelf)
                disconnectedTMP.gameObject.SetActive(false);
        }
        else
        {
            if (connectedTMP != null && connectedTMP.gameObject.activeSelf)
                connectedTMP.gameObject.SetActive(false);
            if (disconnectedTMP != null && !disconnectedTMP.gameObject.activeSelf)
                disconnectedTMP.gameObject.SetActive(true);
        }
    }
}