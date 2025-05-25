using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ModalBlocker : MonoBehaviour
{
    // Singleton pour accès facile
    public static ModalBlocker Instance;
    
    [Header("Configuration")]
    [Tooltip("Les objets à observer - quand l'un d'eux est activé, le bloqueur s'active")]
    [SerializeField] private GameObject[] modalsToWatch;
    
    [Tooltip("Activer automatiquement le bloqueur quand un modal est visible")]
    [SerializeField] private bool autoBlockOnModalActive = true;
    
    [Header("AppKit Integration")]
    [Tooltip("Surveiller également le modal d'AppKit")]
    [SerializeField] private bool watchAppKitModal = true;
    
    [Tooltip("Nom du conteneur modal d'AppKit à rechercher")]
    [SerializeField] private string appKitModalName = "AppKit_ModalContainer";
    
    [Tooltip("Intervalle de vérification pour le modal AppKit (secondes)")]
    [SerializeField] private float checkInterval = 0.2f;
    
    // Référence au panneau qui sera créé automatiquement
    private GameObject blockingPanel;
    
    // Liste pour stocker les références aux composants désactivés
    private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
    
    // Pour la vérification périodique
    private float lastCheckTime = 0f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateBlockingPanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (autoBlockOnModalActive)
        {
            bool anyModalActive = false;
            
            // Vérifier les modaux configurés manuellement
            if (modalsToWatch != null)
            {
                foreach (GameObject modal in modalsToWatch)
                {
                    if (modal != null && modal.activeInHierarchy)
                    {
                        anyModalActive = true;
                        break;
                    }
                }
            }
            
            // Vérifier périodiquement le modal AppKit
            if (!anyModalActive && watchAppKitModal && Time.time > lastCheckTime + checkInterval)
            {
                lastCheckTime = Time.time;
                
                // Rechercher le modal AppKit par son nom
                GameObject appKitModal = GameObject.Find(appKitModalName);
                if (appKitModal != null && appKitModal.activeInHierarchy)
                {
                    anyModalActive = true;
                }
            }
            
            // Activer/désactiver le bloqueur en fonction de l'état des modals
            if (anyModalActive && blockingPanel != null && !blockingPanel.activeInHierarchy)
            {
                EnableBlocker();
            }
            else if (!anyModalActive && blockingPanel != null && blockingPanel.activeInHierarchy)
            {
                DisableBlocker();
            }
        }
    }
    
    // Méthode qui crée le panneau de blocage automatiquement
    private void CreateBlockingPanel()
    {
        // Créer un GameObject pour le panneau
        blockingPanel = new GameObject("ModalBlockingPanel");
        blockingPanel.transform.SetParent(transform);
        
        // Ajouter un Canvas pour s'assurer qu'il est rendu au-dessus de tout
        Canvas canvas = blockingPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // S'assurer qu'il est au-dessus de tout
        
        // Ajouter un CanvasScaler pour s'adapter à tous les écrans
        CanvasScaler scaler = blockingPanel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        
        // Ajouter un GraphicRaycaster pour bloquer les raycast
        blockingPanel.AddComponent<GraphicRaycaster>();
        
        // Créer un Panel transparent qui bloque les interactions
        GameObject panel = new GameObject("BlockerPanel");
        panel.transform.SetParent(blockingPanel.transform, false);
        
        // Ajouter une image transparente qui couvre tout l'écran
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.01f); // Presque invisible mais bloque les raycast
        
        // Configurer le RectTransform pour couvrir tout l'écran
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        // Désactiver le panneau par défaut
        blockingPanel.SetActive(false);
    }
    
    // Méthode publique pour activer le bloqueur
    public void EnableBlocker()
    {
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(true);
            blockingPanel.transform.SetAsLastSibling();
            Debug.Log("ModalBlocker: Interactions bloquées");
            
            // CORRECTION: Désactiver tous les FeatureInteraction
            DisableAllFeatureInteractions();
        }
    }
    
    // Méthode publique pour désactiver le bloqueur
    public void DisableBlocker()
    {
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(false);
            Debug.Log("ModalBlocker: Interactions débloquées");
            
            // CORRECTION: Réactiver tous les FeatureInteraction désactivés
            EnableAllFeatureInteractions();
        }
    }
    
    // CORRECTION: Méthode pour désactiver tous les FeatureInteraction
    private void DisableAllFeatureInteractions()
    {
        // Vider la liste pour la nouvelle session
        disabledComponents.Clear();
        
        // Récupérer tous les FeatureInteraction via réflexion pour éviter les problèmes de namespace
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour mb in allBehaviours)
        {
            // Vérifier si c'est un FeatureInteraction par son nom de type
            if (mb.GetType().Name == "FeatureInteraction" && mb.enabled)
            {
                // CORRECTION: Désactiver le composant et l'ajouter à la liste
                mb.enabled = false;
                disabledComponents.Add(mb);
                Debug.Log($"ModalBlocker: Désactivation de {mb.name} ({mb.GetType().Name})");
            }
            
            // Même chose pour MapGameMapInteractions
            if (mb.GetType().Name == "MapGameMapInteractions" && mb.enabled)
            {
                // CORRECTION: Désactiver le composant et l'ajouter à la liste
                mb.enabled = false;
                disabledComponents.Add(mb);
                Debug.Log($"ModalBlocker: Désactivation de {mb.name} ({mb.GetType().Name})");
            }
        }
        
        Debug.Log($"ModalBlocker: {disabledComponents.Count} composants d'interaction désactivés");
    }
    
    // CORRECTION: Méthode pour réactiver les composants désactivés
    private void EnableAllFeatureInteractions()
    {
        foreach (MonoBehaviour mb in disabledComponents)
        {
            if (mb != null)
            {
                // CORRECTION: Réactiver le composant
                mb.enabled = true;
                Debug.Log($"ModalBlocker: Réactivation de {mb.name} ({mb.GetType().Name})");
            }
        }
        
        // Vider la liste
        disabledComponents.Clear();
        Debug.Log("ModalBlocker: Tous les composants d'interaction ont été réactivés");
    }
}