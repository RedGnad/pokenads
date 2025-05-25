using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AppKitModalDetector : MonoBehaviour
{
    [Header("Objets modaux à surveiller")]
    [Tooltip("Glissez ici les objets modaux ou Canvas à surveiller")]
    [SerializeField] private GameObject[] modalObjectsToWatch;
    
    [Header("Configuration avancée")]
    [Tooltip("Rechercher aussi par nom (moins performant mais utile pour modaux dynamiques)")]
    [SerializeField] private bool searchByNameAsFallback = true;
    
    [Tooltip("Éléments visuels qui indiquent qu'un modal est ouvert (si recherche par nom activée)")]
    [SerializeField] private string[] modalUIElementNames = {
        "AppKit_ModalContainer", "WalletConnectModal"
    };

    [Header("Types d'interactions à désactiver")]
    [Tooltip("Désactiver les colliders")]
    [SerializeField] private bool disableColliders = true;
    
    [Tooltip("Désactiver les scripts d'interaction")]
    [SerializeField] private bool disableScripts = true;
    
    [Header("Scripts spécifiques")]
    [Tooltip("Noms des types de scripts à désactiver")]
    [SerializeField] private string[] scriptNamesToDisable = {
        "FeatureInteraction", "MapGameMapInteractions", "InteractiveObject"
    };
    
    // Pour l'état du modal
    private bool isModalActive = false;
    
    // Liste des GameObjects 3D pour les réactiver plus tard
    private List<Collider> disabledColliders = new List<Collider>();
    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    
    void Start()
    {
        // Commencer à vérifier après un court délai (pour éviter de surcharger le démarrage)
        StartCoroutine(DelayedChecking());
    }
    
    private IEnumerator DelayedChecking()
    {
        // Attendre que la scène soit complètement chargée
        yield return new WaitForSeconds(1.0f);
        
        while (true)
        {
            bool modalDetected = CheckForModals();
            
            // Si l'état a changé
            if (modalDetected != isModalActive)
            {
                isModalActive = modalDetected;
                
                if (modalDetected)
                {
                    DisableAllInteractions();
                }
                else
                {
                    EnableAllInteractions();
                }
            }
            
            // Vérifier à une fréquence raisonnable
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private bool CheckForModals()
    {
        // 1. Vérifier d'abord les objets assignés directement (plus efficace)
        if (modalObjectsToWatch != null && modalObjectsToWatch.Length > 0)
        {
            foreach (GameObject modalObject in modalObjectsToWatch)
            {
                if (modalObject != null && modalObject.activeInHierarchy)
                {
                    // Si un modal est actif, pas besoin de vérifier les autres
                    return true;
                }
            }
        }
        
        // 2. Si aucun modal n'est trouvé et la recherche par nom est activée
        if (searchByNameAsFallback)
        {
            // Vérifier les modaux par nom
            if (modalUIElementNames != null && modalUIElementNames.Length > 0)
            {
                foreach (string elementName in modalUIElementNames)
                {
                    if (string.IsNullOrEmpty(elementName))
                        continue;
                        
                    GameObject element = GameObject.Find(elementName);
                    if (element != null && element.activeInHierarchy)
                    {
                        return true;
                    }
                }
            }
            
            // Vérifier les Canvas avec des noms spécifiques (recherche moins fréquente)
            if (Time.frameCount % 60 == 0) // Une fois par seconde environ
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (!obj.activeInHierarchy)
                        continue;
                        
                    // Vérifier si le nom contient des termes spécifiques
                    string name = obj.name.ToLower();
                    if (name.Contains("modal") || name.Contains("wallet") || name.Contains("connect"))
                    {
                        // Vérifier si c'est un Canvas actif ou s'il contient un Canvas actif
                        Canvas canvas = obj.GetComponent<Canvas>();
                        if (canvas != null)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }
    
    private void DisableAllInteractions()
    {
        Debug.Log("AppKitModalDetector: Modal détecté - Désactivation des interactions 3D");
        
        // Option 1: Désactiver tous les colliders
        if (disableColliders)
        {
            Collider[] allColliders = FindObjectsOfType<Collider>();
            foreach (var collider in allColliders)
            {
                if (collider != null && collider.enabled)
                {
                    collider.enabled = false;
                    disabledColliders.Add(collider);
                }
            }
            Debug.Log($"AppKitModalDetector: {disabledColliders.Count} colliders désactivés");
        }
        
        // Option 2: Désactiver tous les scripts d'interaction connus
        if (disableScripts && scriptNamesToDisable != null && scriptNamesToDisable.Length > 0)
        {
            MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var script in allBehaviours)
            {
                string typeName = script.GetType().Name;
                
                bool shouldDisable = false;
                foreach (string scriptName in scriptNamesToDisable)
                {
                    if (typeName == scriptName)
                    {
                        shouldDisable = true;
                        break;
                    }
                }
                
                if (shouldDisable && script.enabled)
                {
                    script.enabled = false;
                    disabledScripts.Add(script);
                }
            }
            Debug.Log($"AppKitModalDetector: {disabledScripts.Count} scripts d'interaction désactivés");
        }
    }
    
    private void EnableAllInteractions()
    {
        Debug.Log("AppKitModalDetector: Modal fermé - Réactivation des interactions 3D");
        
        // Réactiver tous les colliders désactivés
        foreach (var collider in disabledColliders)
        {
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
        disabledColliders.Clear();
        
        // Réactiver tous les scripts désactivés
        foreach (var script in disabledScripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }
        disabledScripts.Clear();
    }
    
    private void OnDestroy()
    {
        // S'assurer que tout est réactivé
        EnableAllInteractions();
    }
}