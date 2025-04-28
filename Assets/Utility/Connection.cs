using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Reown.AppKit.Unity;
using TMPro;
using System;

public class WalletConnectionHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Button connectButton;
    [SerializeField] private TextMeshProUGUI walletText;
    
    [Header("Configuration")]
    [SerializeField] private bool hideWhenConnected = true;
    
    [Header("Position")]
    [SerializeField] private bool positionnerEnHautADroite = true;
    [SerializeField] private float margeHorizontale = 120f;  // Augmenté à 120f (20f + 100f)
    [SerializeField] private float margeVerticale = 120f;    // Augmenté à 120f (20f + 100f)
    
    [Tooltip("Décalage supplémentaire depuis le bord droit")]
    [SerializeField] private float decalageHorizontal = 0f;
    [Tooltip("Décalage supplémentaire depuis le haut")]
    [SerializeField] private float decalageVertical = 0f;
    
    [Header("Ancrage alternatif")]
    [Tooltip("Utiliser un autre coin pour l'ancrage du bouton")]
    [SerializeField] private AnchorPosition positionAncrage = AnchorPosition.HautDroite;
    
    private string walletAddress = "";
    
    // Propriété statique pour accéder à l'adresse de wallet depuis n'importe quel script
    public static string CurrentWalletAddress { get; private set; } = "";
    
    public static bool ButtonClicked { get; private set; }
    
    public enum AnchorPosition
    {
        HautDroite,
        HautGauche,
        BasDroite,
        BasGauche,
        Centre
    }
    
    private void Start()
    {
        ButtonClicked = false;
        
        if (connectButton == null)
            connectButton = GetComponent<Button>();
            
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OpenConnectModal);
            
            UpdateButtonState();
        }
        
        if (AppKit.IsInitialized)
        {
            AppKit.AccountConnected += OnAccountConnected;
            AppKit.AccountDisconnected += OnAccountDisconnected;
            Debug.Log("ConnectWalletButton: Abonnement aux événements AppKit");
        }
        else
        {
            Debug.LogWarning("ConnectWalletButton: AppKit n'est pas initialisé, le bouton ne fonctionnera pas");
        }
        
        if (positionnerEnHautADroite)
        {
            RepositionnerBouton();
        }
    }
    
    private void Update()
    {
        if (positionnerEnHautADroite && Time.frameCount % 30 == 0)
        {
            RepositionnerBouton();
        }
    }
    
    private void RepositionnerBouton()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                switch (positionAncrage)
                {
                    case AnchorPosition.HautDroite:
                        rectTransform.anchorMin = new Vector2(1, 1);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        rectTransform.pivot = new Vector2(1, 1);
                        rectTransform.anchoredPosition = new Vector2(
                            -(margeHorizontale + decalageHorizontal), 
                            -(margeVerticale + decalageVertical));
                        break;
                    
                    case AnchorPosition.HautGauche:
                        rectTransform.anchorMin = new Vector2(0, 1);
                        rectTransform.anchorMax = new Vector2(0, 1);
                        rectTransform.pivot = new Vector2(0, 1);
                        rectTransform.anchoredPosition = new Vector2(
                            margeHorizontale + decalageHorizontal, 
                            -(margeVerticale + decalageVertical));
                        break;
                    
                    case AnchorPosition.BasDroite:
                        rectTransform.anchorMin = new Vector2(1, 0);
                        rectTransform.anchorMax = new Vector2(1, 0);
                        rectTransform.pivot = new Vector2(1, 0);
                        rectTransform.anchoredPosition = new Vector2(
                            -(margeHorizontale + decalageHorizontal), 
                            margeVerticale + decalageVertical);
                        break;
                    
                    case AnchorPosition.BasGauche:
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0, 0);
                        rectTransform.anchoredPosition = new Vector2(
                            margeHorizontale + decalageHorizontal, 
                            margeVerticale + decalageVertical);
                        break;
                    
                    case AnchorPosition.Centre:
                        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                        rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        rectTransform.anchoredPosition = new Vector2(
                            decalageHorizontal, 
                            decalageVertical);
                        break;
                }
                
                Debug.Log($"ConnectWalletButton: Bouton wallet repositionné: {rectTransform.anchoredPosition}, position: {positionAncrage}");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (AppKit.IsInitialized)
        {
            AppKit.AccountConnected -= OnAccountConnected;
            AppKit.AccountDisconnected -= OnAccountDisconnected;
            Debug.Log("ConnectWalletButton: Désabonnement des événements AppKit");
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        ButtonClicked = true;
        
        CancelInvoke("ResetButtonClickedFlag");
        Invoke("ResetButtonClickedFlag", 0.2f);
        
        Debug.Log("ConnectWalletButton: Clic sur le bouton wallet détecté et traité");
    }
    
    private void ResetButtonClickedFlag()
    {
        ButtonClicked = false;
    }
    
    public void OpenConnectModal()
    {
        if (AppKit.IsInitialized)
        {
            ButtonClicked = true;
            
            AppKit.OpenModal();
            Debug.Log("ConnectWalletButton: Modal de connexion ouvert");
            
            CancelInvoke("ResetButtonClickedFlag");
            Invoke("ResetButtonClickedFlag", 0.2f);
        }
        else
        {
            Debug.LogError("ConnectWalletButton: AppKit n'est pas initialisé");
        }
    }
    
    private void UpdateButtonState()
    {
        if (AppKit.IsInitialized && AppKit.IsAccountConnected)
        {
            if (hideWhenConnected)
                connectButton.gameObject.SetActive(false);
            else
                connectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Wallet Connecté";
        }
        else
        {
            connectButton.gameObject.SetActive(true);
            connectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Wallet";
        }
    }
    
    private async void OnAccountConnected(object sender, EventArgs e)
    {
        if (AppKit.IsAccountConnected)
        {
            var account = await AppKit.GetAccountAsync();
            walletAddress = account.Address;
            
            // Mettre à jour la propriété statique pour un accès global
            CurrentWalletAddress = walletAddress;
            
            if (walletText != null)
            {
                string formattedAddress = FormatAddress(walletAddress);
                walletText.text = formattedAddress;
            }
            
            UpdateButtonState();
            
            // Remplacé le code GameRelayer par un simple log
            Debug.Log($"ConnectWalletButton: Wallet connecté - {walletAddress}");
        }
    }
    
    private void OnAccountDisconnected(object sender, EventArgs e)
    {
        walletAddress = "";
        CurrentWalletAddress = ""; // Vider aussi la propriété statique
        
        if (walletText != null)
            walletText.text = "";
            
        UpdateButtonState();
        
        Debug.Log("ConnectWalletButton: Wallet déconnecté");
    }
    
    private string FormatAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length <= 10)
            return address;
                
        return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
    }
}