using UnityEngine;
using TMPro;
using Reown.AppKit.Unity; // pour WalletManager

/// <summary>
/// Affiche le texte « Nads: X » quand le wallet est connecté,
/// ou le texte « Score : 0 » quand il est déconnecté.
/// </summary>
public class ScoreDisplayToggle : MonoBehaviour
{
    [Header("Drag & drop vos deux TextMeshProUGUI")]
    public TextMeshProUGUI personalizedTMP;   // « Nads: X »
    public TextMeshProUGUI generalTMP;        // « Score : 0 »

    void Start()
    {
        // Si vous ne les avez pas assignés, on tente de les trouver par nom
        if (personalizedTMP == null)
        {
            var go = GameObject.Find("PersonalizedScoreText");
            if (go != null) personalizedTMP = go.GetComponent<TextMeshProUGUI>();
        }
        if (generalTMP == null)
        {
            var go = GameObject.Find("GeneralScoreText");
            if (go != null) generalTMP = go.GetComponent<TextMeshProUGUI>();
        }

        if (personalizedTMP == null || generalTMP == null)
            Debug.LogWarning("[ScoreDisplayToggle] Pensez à assigner vos deux TMP en Inspector !");
    }

    void Update()
    {
        // Polling à chaque frame
        bool connected = !string.IsNullOrEmpty(WalletManager.CurrentWalletAddress);

        if (personalizedTMP != null)
            personalizedTMP.gameObject.SetActive(connected);

        if (generalTMP != null)
            generalTMP.gameObject.SetActive(!connected);
    }
}