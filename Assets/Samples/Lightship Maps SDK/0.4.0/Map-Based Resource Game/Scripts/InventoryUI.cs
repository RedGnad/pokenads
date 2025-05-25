using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Niantic.Lightship.Maps.Samples.GameSample;

public class InventoryUI : MonoBehaviour
{
    // Flag global pour indiquer si l'inventaire est ouvert
    public static bool IsInventoryOpen = false;

    [Header("Panels principaux")]
    public GameObject inventoryPanel;
    public GameObject monstersPanel;
    public GameObject weaponsPanel;
    public GameObject othersPanel;
    
    [Header("Boutons d'onglets")]
    public Button monstersTabButton;
    public Button weaponsTabButton;
    public Button othersTabButton;
    
    [Header("Container des onglets et bouton de fermeture")]
    public GameObject tabButtonsContainer;  // Container pour les boutons d'onglets
    public Button closeTabButton;           // Bouton de fermeture de l'onglet actif
    
    [Header("Textes d'inventaire des monstres")]
    public TextMeshProUGUI mouchCountText;
    public TextMeshProUGUI chogCountText;
    public TextMeshProUGUI moyakiCountText;
    public TextMeshProUGUI molandakCountText; // Nouveau compteur pour Molandak
    
    // Références gardées pour compatibilité, mais on ne les utilisera plus directement
    private FeatureInteraction[] featureInteractions;
    private MapGameMapInteractions mapInteractions;
    
    // Référence au panel actuellement ouvert
    private GameObject currentActivePanel;

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
            
        // S'assurer que les panels sont désactivés au démarrage
        if (monstersPanel != null) monstersPanel.SetActive(false);
        if (weaponsPanel != null) weaponsPanel.SetActive(false);
        if (othersPanel != null) othersPanel.SetActive(false);
        
        // S'assurer que le bouton de fermeture est masqué au démarrage
        if (closeTabButton != null) 
            closeTabButton.gameObject.SetActive(false);
        
        // Configurer les boutons d'onglets
        if (monstersTabButton != null)
            monstersTabButton.onClick.AddListener(() => ShowPanel(monstersPanel));
        
        if (weaponsTabButton != null)
            weaponsTabButton.onClick.AddListener(() => ShowPanel(weaponsPanel));
        
        if (othersTabButton != null)
            othersTabButton.onClick.AddListener(() => ShowPanel(othersPanel));
            
        // Configurer le bouton de fermeture
        if (closeTabButton != null)
            closeTabButton.onClick.AddListener(CloseCurrentPanel);
            
        // Trouver tous les scripts d'interaction dans la scène (pour référence)
        featureInteractions = FindObjectsOfType<FeatureInteraction>();
        mapInteractions = FindObjectOfType<MapGameMapInteractions>();
        
        Debug.Log($"[InventoryUI] Trouvé {featureInteractions.Length} FeatureInteraction et {(mapInteractions != null ? "1" : "0")} MapGameMapInteractions");
        
        // S'assurer que le flag est correctement initialisé au démarrage
        IsInventoryOpen = false;
    }

    // Affiche un panel spécifique et masque les autres
    private void ShowPanel(GameObject panelToShow)
    {
        // Enregistrer le panel actif
        currentActivePanel = panelToShow;
        
        // Masquer tous les panels sauf celui à afficher
        if (monstersPanel != null) monstersPanel.SetActive(monstersPanel == panelToShow);
        if (weaponsPanel != null) weaponsPanel.SetActive(weaponsPanel == panelToShow);
        if (othersPanel != null) othersPanel.SetActive(othersPanel == panelToShow);
        
        // Masquer les boutons d'onglets et afficher le bouton de fermeture
        if (tabButtonsContainer != null) 
            tabButtonsContainer.SetActive(false);
            
        if (closeTabButton != null) 
            closeTabButton.gameObject.SetActive(true);
        
        // Mettre à jour l'interface si c'est le panel des monstres
        if (panelToShow == monstersPanel)
            UpdateMonsterUI();
            
        // Vider l'état de sélection pour éviter les clics indésirables
        EventSystem.current.SetSelectedGameObject(null);
        
        Debug.Log($"[InventoryUI] Panel {panelToShow.name} affiché, bouton de fermeture activé: {closeTabButton != null && closeTabButton.gameObject.activeInHierarchy}");
    }
    
    // Ferme le panel actif et réaffiche les onglets
    public void CloseCurrentPanel()
    {
        // Masquer tous les panels
        if (monstersPanel != null) monstersPanel.SetActive(false);
        if (weaponsPanel != null) weaponsPanel.SetActive(false);
        if (othersPanel != null) othersPanel.SetActive(false);
        
        // Réafficher les boutons d'onglets et masquer le bouton de fermeture
        if (tabButtonsContainer != null) 
            tabButtonsContainer.SetActive(true);
            
        if (closeTabButton != null) 
            closeTabButton.gameObject.SetActive(false);
        
        currentActivePanel = null;
        
        Debug.Log("[InventoryUI] Panels fermés, boutons d'onglets réaffichés");
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool state = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(state);
            
            // MODIFICATION: Utiliser un flag global au lieu de désactiver les scripts
            IsInventoryOpen = state;
            Debug.Log($"[InventoryUI] Inventaire {(state ? "OUVERT" : "FERMÉ")}, interactions bloquées: {state}");
            
            if (state)
            {
                // Si on ouvre l'inventaire, s'assurer qu'aucun panel n'est ouvert
                // et que les boutons d'onglets sont visibles
                if (monstersPanel != null) monstersPanel.SetActive(false);
                if (weaponsPanel != null) weaponsPanel.SetActive(false);
                if (othersPanel != null) othersPanel.SetActive(false);
                
                if (tabButtonsContainer != null) 
                    tabButtonsContainer.SetActive(true);
                    
                if (closeTabButton != null) 
                    closeTabButton.gameObject.SetActive(false);
                
                currentActivePanel = null;
            }
        }
    }
    
    // Mise à jour spécifique pour le panel des monstres
    public void UpdateMonsterUI()
    {
        if (GameManager.Instance != null)
        {
            int mouchCount = 0;
            int chogCount = 0;
            int moyakiCount = 0;
            int molandakCount = 0; // Nouveau compteur pour Molandak
            int count;
            
            if (GameManager.Instance.capturedMonsters.TryGetValue("Mouch", out count))
                mouchCount = count;
            if (GameManager.Instance.capturedMonsters.TryGetValue("Chog", out count))
                chogCount = count;
            if (GameManager.Instance.capturedMonsters.TryGetValue("Moyaki", out count))
                moyakiCount = count;
            if (GameManager.Instance.capturedMonsters.TryGetValue("Molandak", out count))
                molandakCount = count;

            if (mouchCountText != null)
                mouchCountText.text = "Mouch : " + mouchCount;
            if (chogCountText != null)
                chogCountText.text = "Skibidi Chog : " + chogCount;
            if (moyakiCountText != null)
                moyakiCountText.text = "Moyaki : " + moyakiCount;
            if (molandakCountText != null)
                molandakCountText.text = "Molandak : " + molandakCount;
        }
    }
    
    // L'ancienne méthode UpdateUI redirige vers UpdateMonsterUI pour compatibilité
    public void UpdateUI()
    {
        UpdateMonsterUI();
    }
}