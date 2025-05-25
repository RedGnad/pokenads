using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Version renforcée qui bloque TOUS les types d'événements d'interaction
public class InventoryBlocker : MonoBehaviour, 
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private void Awake()
    {
        // Configuration immédiate pour s'assurer que tout est prêt avant même Start()
        ConfigureBlocker();
    }
    
    void Start()
    {
        // Double vérification au Start() aussi
        ConfigureBlocker();
    }
    
    private void ConfigureBlocker()
    {
        // 1. Vérifier/ajouter l'Image pour bloquer les raycast
        Image panelImage = GetComponent<Image>();
        if (panelImage == null)
        {
            // Ajouter une image si elle n'existe pas
            panelImage = gameObject.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.01f); // Presque invisible
        }
        panelImage.raycastTarget = true;
        
        // 2. Ajouter un CanvasGroup pour plus de sécurité
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
        group.blocksRaycasts = true;
        
        // 3. S'assurer que le RectTransform couvre toute la zone
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        Debug.Log("[InventoryBlocker] Bloqueur configuré avec succès");
    }
    
    // Intercepter et bloquer tous les types d'événements
    public void OnPointerClick(PointerEventData eventData)
    {
        eventData.Use();
        Debug.Log("[InventoryBlocker] Clic bloqué");
    }
    
    public void OnPointerDown(PointerEventData eventData) 
    {
        eventData.Use(); 
    }
    
    public void OnPointerUp(PointerEventData eventData) 
    {
        eventData.Use(); 
    }
    
    public void OnBeginDrag(PointerEventData eventData) 
    {
        eventData.Use(); 
    }
    
    public void OnDrag(PointerEventData eventData) 
    {
        eventData.Use(); 
    }
    
    public void OnEndDrag(PointerEventData eventData) 
    {
        eventData.Use(); 
    }
}