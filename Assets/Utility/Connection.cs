using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Reown.AppKit.Unity;
using TMPro;
using System;
using System.Collections.Generic;

public class WalletConnectionHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Button connectButton;
    [SerializeField] private TextMeshProUGUI walletText;
    
    [Header("Configuration")]
    [SerializeField] private bool hideWhenConnected = true;
    
    [Header("Position")]
    [SerializeField] private bool positionnerEnHautADroite = true;
    [SerializeField] private float margeHorizontale = 120f;
    [SerializeField] private float margeVerticale = 120f;
    
    [Tooltip("Décalage supplémentaire depuis le bord droit")]
    [SerializeField] private float decalageHorizontal = 0f;
    [Tooltip("Décalage supplémentaire depuis le haut")]
    [SerializeField] private float decalageVertical = 0f;
    
    [Header("Ancrage alternatif")]
    [Tooltip("Utiliser un autre coin pour l'ancrage du bouton")]
    [SerializeField] private AnchorPosition positionAncrage = AnchorPosition.HautDroite;
    
    [Header("Hitbox Configuration")]
    [Tooltip("Taille des hitbox de fermeture")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(100, 100);
    
    [Tooltip("Positions des hitbox (pourcentage de l'écran, 0-1)")]
    [SerializeField] private Vector2[] hitboxPositions = new Vector2[] {
        new Vector2(0.95f, 0.95f),  // Haut droite (X)
        new Vector2(0.5f, 0.15f)    // Bas centre (Cancel)
    };
    
    [Tooltip("Décalages des hitbox en pixels (ajustement fin)")]
    [SerializeField] private Vector2[] hitboxOffsets = new Vector2[] {
        new Vector2(0, 0),      // Offset pour la première hitbox
        new Vector2(0, 0)       // Offset pour la deuxième hitbox
    };
    
    [Tooltip("Couleur des hitbox (transparente pour production)")]
    [SerializeField] private Color hitboxColor = new Color(1, 0, 0, 0.1f);
    
    private string walletAddress = "";
    private bool firstRepositioning = true;
    private List<GameObject> activeHitboxes = new List<GameObject>();
    private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
    private GameObject hitboxCanvas;
    
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
        
        // Initialiser le canvas pour les hitbox
        InitializeHitboxCanvas();
        
        // S'assurer que le tableau des offsets contient assez d'éléments
        if (hitboxOffsets.Length < hitboxPositions.Length)
        {
            Vector2[] newOffsets = new Vector2[hitboxPositions.Length];
            for (int i = 0; i < hitboxPositions.Length; i++)
            {
                if (i < hitboxOffsets.Length)
                    newOffsets[i] = hitboxOffsets[i];
                else
                    newOffsets[i] = Vector2.zero;
            }
            hitboxOffsets = newOffsets;
        }
    }
    
    private void InitializeHitboxCanvas()
    {
        // Supprimer tout canvas existant
        if (hitboxCanvas != null)
        {
            Destroy(hitboxCanvas);
        }
        
        // Créer un nouveau canvas pour les hitbox
        hitboxCanvas = new GameObject("HitboxCanvas");
        Canvas canvas = hitboxCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10001; // Au-dessus du modal
        
        CanvasScaler scaler = hitboxCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        hitboxCanvas.AddComponent<GraphicRaycaster>();
        
        // Le rendre persistant
        DontDestroyOnLoad(hitboxCanvas);
        
        // Le désactiver par défaut
        hitboxCanvas.SetActive(false);
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
                
                if (firstRepositioning)
                {
                    Debug.Log($"ConnectWalletButton: Bouton wallet positionné: {rectTransform.anchoredPosition}, position: {positionAncrage}");
                    firstRepositioning = false;
                }
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
        
        // Nettoyer les hitbox
        CleanupHitboxes();
        
        // Nettoyer le canvas
        if (hitboxCanvas != null)
        {
            Destroy(hitboxCanvas);
        }
        
        // S'assurer que le flag du modal est réinitialisé
        WalletModalState.IsModalOpen = false;
        
        // S'assurer que toutes les interactions sont réactivées
        ReenableInteractions();
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
    
    // Méthode appelée lors du clic sur le bouton de connexion wallet
    public void OpenConnectModal()
    {
        if (AppKit.IsInitialized)
        {
            ButtonClicked = true;
            
            // Activer le flag global
            WalletModalState.IsModalOpen = true;
            Debug.Log("ConnectWalletButton: Modal ouvert - Interactions avec features bloquées");
            
            // Désactiver les interactions 3D
            DisableInteractions();
            
            // Créer les hitbox pour détecter la fermeture du modal
            CreateHitboxes();
            
            // Ouvrir le modal
            AppKit.OpenModal();
            
            CancelInvoke("ResetButtonClickedFlag");
            Invoke("ResetButtonClickedFlag", 0.2f);
        }
        else
        {
            Debug.LogError("ConnectWalletButton: AppKit n'est pas initialisé");
        }
    }
    
    // Méthode pour créer les hitbox - MODIFIÉE pour permettre aux clics de traverser
    private void CreateHitboxes()
    {
        // Nettoyer les hitbox existantes
        CleanupHitboxes();
        
        // Activer le canvas
        if (hitboxCanvas != null)
        {
            hitboxCanvas.SetActive(true);
        }
        
        // Créer des hitbox aux positions spécifiées
        for (int i = 0; i < hitboxPositions.Length; i++)
        {
            Vector2 positionRatio = hitboxPositions[i];
            Vector2 offset = (i < hitboxOffsets.Length) ? hitboxOffsets[i] : Vector2.zero;
            
            // Créer un gameobject pour la hitbox
            GameObject hitbox = new GameObject($"ModalCloseHitbox_{i}");
            hitbox.transform.SetParent(hitboxCanvas.transform, false);
            
            // Ajouter une image semi-transparente
            Image hitboxImage = hitbox.AddComponent<Image>();
            hitboxImage.color = hitboxColor;
            // MODIFICATION: Rendre l'image non raycastable pour que les clics traversent
            hitboxImage.raycastTarget = false;
            
            // Configurer le RectTransform
            RectTransform rectTransform = hitbox.GetComponent<RectTransform>();
            rectTransform.anchorMin = positionRatio;
            rectTransform.anchorMax = positionRatio;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = hitboxSize;
            
            // Appliquer l'offset pour un positionnement précis
            rectTransform.anchoredPosition = offset;
            
            // Ajouter notre détecteur de clics qui n'intercepte pas les clics
            HitboxClickDetector detector = hitbox.AddComponent<HitboxClickDetector>();
            detector.Initialize(rectTransform, OnHitboxClicked);
            
            // Conserver une référence
            activeHitboxes.Add(hitbox);
            
            Debug.Log($"ConnectWalletButton: Hitbox {i} créée à {positionRatio} avec offset {offset}");
        }
    }
    
    // Méthode appelée quand une hitbox est cliquée
    public void OnHitboxClicked()
    {
        Debug.Log("ConnectWalletButton: Hitbox cliquée - Fermeture du modal détectée");
        
        // Réactiver les interactions
        ReenableInteractions();
        
        // Nettoyer les hitbox
        CleanupHitboxes();
        
        // Désactiver le canvas
        if (hitboxCanvas != null)
        {
            hitboxCanvas.SetActive(false);
        }
    }
    
    // Méthode pour nettoyer les hitbox
    private void CleanupHitboxes()
    {
        foreach (GameObject hitbox in activeHitboxes)
        {
            if (hitbox != null)
            {
                Destroy(hitbox);
            }
        }
        
        activeHitboxes.Clear();
    }
    
    // Méthode pour désactiver les interactions 3D
    private void DisableInteractions()
    {
        // Vider la liste pour éviter les doublons
        disabledComponents.Clear();
        
        // Trouver tous les comportements actifs
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
        
        foreach (MonoBehaviour mb in allBehaviours)
        {
            if (mb == null) continue;
            
            string typeName = mb.GetType().Name;
            
            // Désactiver les scripts d'interaction connus
            if ((typeName == "FeatureInteraction" || typeName == "MapGameMapInteractions") && mb.enabled)
            {
                mb.enabled = false;
                disabledComponents.Add(mb);
                Debug.Log($"ConnectWalletButton: Désactivation de {mb.gameObject.name} ({typeName})");
            }
        }
        
        Debug.Log($"ConnectWalletButton: {disabledComponents.Count} interactions désactivées jusqu'à fermeture du modal");
    }
    
    // Méthode pour réactiver les interactions 3D
    private void ReenableInteractions()
    {
        foreach (MonoBehaviour mb in disabledComponents)
        {
            if (mb != null)
            {
                mb.enabled = true;
                Debug.Log($"ConnectWalletButton: Réactivation de {mb.gameObject.name} ({mb.GetType().Name})");
            }
        }
        
        // Vider la liste
        disabledComponents.Clear();
        
        // Désactiver le flag global
        WalletModalState.IsModalOpen = false;
        
        Debug.Log("ConnectWalletButton: Toutes les interactions ont été réactivées");
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
    
    // Méthode appelée quand le compte est connecté
    private async void OnAccountConnected(object sender, EventArgs e)
    {
        if (AppKit.IsAccountConnected)
        {
            var account = await AppKit.GetAccountAsync();
            walletAddress = account.Address;
            
            // Mettre à jour la propriété statique
            CurrentWalletAddress = walletAddress;
            
            // Réactiver immédiatement toutes les interactions
            CancelInvoke("ReenableInteractions");
            ReenableInteractions();
            
            // Nettoyer les hitbox
            CleanupHitboxes();
            
            // Désactiver le canvas
            if (hitboxCanvas != null)
            {
                hitboxCanvas.SetActive(false);
            }
            
            // Mettre à jour l'UI
            if (walletText != null)
            {
                string formattedAddress = FormatAddress(walletAddress);
                walletText.text = formattedAddress;
            }
            
            UpdateButtonState();
            
            Debug.Log($"ConnectWalletButton: Wallet connecté - {walletAddress}");
        }
    }
    
    // Méthode appelée quand le compte est déconnecté
    private void OnAccountDisconnected(object sender, EventArgs e)
    {
        walletAddress = "";
        CurrentWalletAddress = "";
        
        // Réactiver immédiatement toutes les interactions
        CancelInvoke("ReenableInteractions");
        ReenableInteractions();
        
        // Nettoyer les hitbox
        CleanupHitboxes();
        
        // Désactiver le canvas
        if (hitboxCanvas != null)
        {
            hitboxCanvas.SetActive(false);
        }
        
        // Mettre à jour l'UI
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
    
    // Détecteur de clics sur les hitbox qui ne bloque pas les événements
    private class HitboxClickDetector : MonoBehaviour
    {
        private Action onClickAction;
        private RectTransform rectTransform;
        private Canvas parentCanvas;
        
        public void Initialize(RectTransform rect, Action onClick)
        {
            rectTransform = rect;
            onClickAction = onClick;
            parentCanvas = GetComponentInParent<Canvas>();
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Vérifier si le clic est dans les limites de cette hitbox
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, parentCanvas?.worldCamera))
                {
                    // Appeler l'action sans bloquer le clic
                    onClickAction?.Invoke();
                }
            }
        }
    }
}