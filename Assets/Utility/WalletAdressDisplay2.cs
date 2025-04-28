using UnityEngine;
using TMPro;

public class WalletAddressDisplay : MonoBehaviour
{
    [Header("Référence au TMP sur la MapScreen")]
    [SerializeField] private TextMeshProUGUI addressText;
    [Header("Texte quand pas connecté")]
    [SerializeField] private string notConnectedText = "Non connecté";
    [Header("Format : full ou tronqué")]
    [SerializeField] private bool useShortFormat = true;
    [Tooltip("6 premiers + ... + 4 derniers")]
    [SerializeField] private int headLength = 6;
    [SerializeField] private int tailLength = 4;

    private void OnEnable()
    {
        // Souscrire à l'event de changement d'adresse
        WalletManager.OnWalletAddressChanged += OnAddressChanged;
        // Mettre à jour tout de suite
        OnAddressChanged(WalletManager.CurrentWalletAddress);
    }

    private void OnDisable()
    {
        WalletManager.OnWalletAddressChanged -= OnAddressChanged;
    }

    private void OnAddressChanged(string newAddr)
    {
        if (addressText == null) return;

        if (string.IsNullOrEmpty(newAddr))
        {
            addressText.text = notConnectedText;
        }
        else if (useShortFormat && newAddr.Length > headLength + tailLength)
        {
            var head = newAddr.Substring(0, headLength);
            var tail = newAddr.Substring(newAddr.Length - tailLength);
            addressText.text = $"{head}…{tail}";
        }
        else
        {
            addressText.text = newAddr;
        }
    }
}