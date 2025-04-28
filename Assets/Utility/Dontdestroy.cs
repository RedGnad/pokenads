using UnityEngine;
using System.Collections;
using System;

public class AppKitPreserver : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string appKitButtonName = "AppKitButton";
    
    // Permet de configurer plusieurs objets à préserver
    [SerializeField] private string[] additionalObjectsToPreserve;
    
    private static AppKitPreserver instance;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Chercher et préserver le bouton AppKit
            StartCoroutine(FindAndPreserveAppKitButton());
            
            // Préserver les objets additionnels configurés
            PreserveAdditionalObjects();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private IEnumerator FindAndPreserveAppKitButton()
    {
        // Attendre un peu pour s'assurer que tous les objets sont initialisés
        yield return new WaitForSeconds(1.0f);
        
        GameObject appKitButton = GameObject.Find(appKitButtonName);
        if (appKitButton != null)
        {
            DontDestroyOnLoad(appKitButton);
            Debug.Log($"[AppKitPreserver] Bouton AppKit '{appKitButtonName}' préservé entre les scènes");
        }
        else
        {
            Debug.LogWarning($"[AppKitPreserver] Bouton AppKit '{appKitButtonName}' non trouvé");
            
            // Essayer de trouver dynamiquement le bouton
            var appKitType = FindType("Reown.AppKit.Unity.AppKit");
            if (appKitType != null)
            {
                var buttonProperty = appKitType.GetProperty("Button");
                if (buttonProperty != null)
                {
                    var button = buttonProperty.GetValue(null) as Component;
                    if (button != null)
                    {
                        DontDestroyOnLoad(button.gameObject);
                        Debug.Log("[AppKitPreserver] Bouton AppKit trouvé par réflexion et préservé");
                    }
                }
            }
        }
    }
    
    private void PreserveAdditionalObjects()
    {
        foreach (string objectName in additionalObjectsToPreserve)
        {
            if (string.IsNullOrEmpty(objectName))
                continue;
                
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
            {
                DontDestroyOnLoad(obj);
                Debug.Log($"[AppKitPreserver] Objet additionnel '{objectName}' préservé entre les scènes");
            }
            else
            {
                Debug.LogWarning($"[AppKitPreserver] Objet additionnel '{objectName}' non trouvé");
            }
        }
    }
    
    private Type FindType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName);
            if (type != null)
                return type;
        }
        return null;
    }
}