using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScaler : MonoBehaviour
{
    [Header("Adaptation à l'écran")]
    [SerializeField] private bool adaptFontSize = true;
    [SerializeField] private float baseFontSize = 24f;
    [SerializeField] private bool adaptButtonSize = true;
    [SerializeField] private Vector2 baseButtonSize = new Vector2(200f, 60f);
    
    [Header("Taille minimale")]
    [SerializeField] private float minFontSize = 14f;
    [SerializeField] private float minButtonWidth = 120f;
    [SerializeField] private float minButtonHeight = 40f;
    
    private readonly Vector2 baseResolution = new Vector2(1920f, 1080f);
    
    private RectTransform rectTransform;
    private TextMeshProUGUI textComponent;
    private CanvasScaler canvasScaler;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasScaler = parentCanvas.GetComponent<CanvasScaler>();
        }
        
        ApplyScaling();
    }
    
    void Update()
    {
        if (Time.frameCount % 60 == 0) // Toutes les 60 frames environ
        {
            ApplyScaling();
        }
    }
    
    void ApplyScaling()
    {
        float scaleFactor = CalculateScaleFactor();
        
        if (adaptButtonSize && rectTransform != null)
        {
            float newWidth = Mathf.Max(baseButtonSize.x * scaleFactor, minButtonWidth);
            float newHeight = Mathf.Max(baseButtonSize.y * scaleFactor, minButtonHeight);
            rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
        }
        
        if (adaptFontSize && textComponent != null)
        {
            textComponent.fontSize = Mathf.Max(baseFontSize * scaleFactor, minFontSize);
        }
    }
    
    float CalculateScaleFactor()
    {
        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            return canvasScaler.scaleFactor;
        }
        
        float widthRatio = Screen.width / baseResolution.x;
        float heightRatio = Screen.height / baseResolution.y;
        
        return Mathf.Min(widthRatio, heightRatio);
    }
}