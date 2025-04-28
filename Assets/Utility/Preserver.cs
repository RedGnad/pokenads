using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class UIPreserver : MonoBehaviour
{
    [SerializeField] private string appKitButtonName = "AppKitButton";
    [SerializeField] private string walletAddressTextName = "WalletAddressText"; 
    [SerializeField] private bool debugMode = true;

    // Singleton pour accès global
    public static UIPreserver Instance { get; private set; }
    
    // Références aux textes et adresses
    private GameObject appKitButtonRef;
    private GameObject walletTextRef;
    private string lastKnownWalletAddress = "";

    private void Awake()
    {
        // Configuration singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // S'abonner aux événements de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Obtenir l'adresse wallet actuelle
        lastKnownWalletAddress = WalletManager.CurrentWalletAddress;
        
        // S'abonner aux changements d'adresse
        WalletManager.OnWalletAddressChanged += CacheWalletAddress;
        
        // Préserver les références initiales
        StartCoroutine(PreserveUIReferences(0.5f));
    }
    
    private void CacheWalletAddress(string address)
    {
        lastKnownWalletAddress = address;
        if (debugMode) Debug.Log($"[UIPreserver] Adresse wallet mise en cache: {address}");
    }

    private IEnumerator PreserveUIReferences(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Garder des références aux objets originaux
        appKitButtonRef = GameObject.Find(appKitButtonName);
        walletTextRef = GameObject.Find(walletAddressTextName);
        
        if (appKitButtonRef != null)
        {
            if (debugMode) Debug.Log($"[UIPreserver] Référence au bouton AppKit obtenue: {appKitButtonName}");
        }
        
        if (walletTextRef != null)
        {
            if (debugMode) Debug.Log($"[UIPreserver] Référence au texte wallet obtenue: {walletAddressTextName}");
            
            // Sauver le texte actuel au cas où
            var tmpText = walletTextRef.GetComponent<TextMeshProUGUI>();
            if (tmpText != null && !string.IsNullOrEmpty(tmpText.text) && tmpText.text != "New Text")
            {
                lastKnownWalletAddress = tmpText.text;
                if (debugMode) Debug.Log($"[UIPreserver] Texte wallet sauvegardé: {lastKnownWalletAddress}");
            }
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (debugMode) Debug.Log($"[UIPreserver] Nouvelle scène chargée: {scene.name}");
        StartCoroutine(RestoreUIElements(1.0f));
    }
    
    private IEnumerator RestoreUIElements(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 1. Restaurer le texte d'adresse wallet
        GameObject newWalletText = GameObject.Find(walletAddressTextName);
        if (newWalletText != null)
        {
            var tmpText = newWalletText.GetComponent<TextMeshProUGUI>();
            if (tmpText != null && (tmpText.text == "New Text" || string.IsNullOrEmpty(tmpText.text)))
            {
                tmpText.text = !string.IsNullOrEmpty(lastKnownWalletAddress) ? lastKnownWalletAddress : "Non connecté";
                if (debugMode) Debug.Log($"[UIPreserver] Texte wallet restauré: {tmpText.text}");
            }
        }
        
        // 2. Forcer un rafraîchissement du WalletManager
        if (WalletManager.Instance != null)
        {
            if (debugMode) Debug.Log("[UIPreserver] Demande de rafraîchissement au WalletManager");
            // On utilise la réflection pour appeler la méthode CheckWalletAddress
            var method = WalletManager.Instance.GetType().GetMethod("CheckWalletAddress", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
                method.Invoke(WalletManager.Instance, null);
            
            // Si la méthode RefreshWalletAddress a été ajoutée, l'appeler directement
            try
            {
                WalletManager.Instance.RefreshWalletAddress();
                if (debugMode) Debug.Log("[UIPreserver] RefreshWalletAddress appelée avec succès");
            }
            catch (System.Exception)
            {
                if (debugMode) Debug.Log("[UIPreserver] La méthode RefreshWalletAddress n'est pas disponible");
            }
        }
        
        // 3. Vérifier si le bouton AppKit doit être restauré
        GameObject appKitButton = GameObject.Find(appKitButtonName);
        if (appKitButton == null)
        {
            if (debugMode) Debug.Log("[UIPreserver] Bouton AppKit non trouvé dans la nouvelle scène");
            // Rechercher le bouton AppKit dans la scène DontDestroyOnLoad
            GameObject tempObj = new GameObject("TempObj");
            DontDestroyOnLoad(tempObj);
            Scene dontDestroyScene = tempObj.scene;
            Destroy(tempObj);
            
            GameObject[] rootObjs = dontDestroyScene.GetRootGameObjects();
            foreach (var obj in rootObjs)
            {
                if (obj.name == appKitButtonName)
                {
                    if (debugMode) Debug.Log("[UIPreserver] Bouton AppKit trouvé dans DontDestroyOnLoad, ré-activation");
                    obj.SetActive(true);
                    break;
                }
                
                Transform foundBtn = obj.transform.Find(appKitButtonName);
                if (foundBtn != null)
                {
                    if (debugMode) Debug.Log("[UIPreserver] Bouton AppKit trouvé dans enfant de DontDestroyOnLoad, ré-activation");
                    foundBtn.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        WalletManager.OnWalletAddressChanged -= CacheWalletAddress;
    }
}