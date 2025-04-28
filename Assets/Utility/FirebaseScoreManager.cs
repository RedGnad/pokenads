using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;

#if FIREBASE_DATABASE
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
#endif

public class FirebaseScoreManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private string defaultScoreText = "Score: 0";
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private bool autoSyncEnabled = true;
    [SerializeField] private float syncInterval = 30f; // secondes
    
    [Header("Firebase Path")]
    [SerializeField] private string databasePath = "users/"; // Chemin dans Firebase
    [SerializeField] private string scoreField = "score"; // Nom du champ contenant le score
    
    private string currentWalletAddress = string.Empty;
    private int currentScore = 0;
    private bool isInitialized = false;

    #if FIREBASE_DATABASE
    private DatabaseReference databaseReference;
    #endif

    void Start()
    {
        if (scoreDisplay == null)
        {
            scoreDisplay = GetComponent<TextMeshProUGUI>();
        }
        
        // Initialiser avec le score par défaut
        if (scoreDisplay != null)
        {
            scoreDisplay.text = defaultScoreText;
        }
        
        // Démarrer l'initialisation de Firebase
        StartCoroutine(InitializeFirebase());
        
        // S'abonner à l'événement de changement d'adresse wallet
        WalletManager.OnWalletAddressChanged += HandleWalletAddressChanged;
        
        // Vérifier si nous avons déjà une adresse wallet (si WalletManager était déjà initialisé)
        if (!string.IsNullOrEmpty(WalletManager.CurrentWalletAddress))
        {
            HandleWalletAddressChanged(WalletManager.CurrentWalletAddress);
        }
    }

    private IEnumerator InitializeFirebase()
    {
        #if FIREBASE_DATABASE
        try
        {
            // Attendre que Firebase soit prêt
            var dependencyStatus = FirebaseApp.CheckAndFixDependenciesAsync();
            yield return new WaitUntil(() => dependencyStatus.IsCompleted);
            
            if (dependencyStatus.Result == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                isInitialized = true;
                Debug.Log("[FirebaseScoreManager] Firebase initialisé avec succès");
                
                // Si nous avons déjà une adresse wallet, charger le score
                if (!string.IsNullOrEmpty(currentWalletAddress))
                {
                    FetchScoreFromFirebase(currentWalletAddress);
                }
                
                // Démarrer la synchronisation automatique si activée
                if (autoSyncEnabled)
                {
                    InvokeRepeating(nameof(SyncScoreWithFirebase), syncInterval, syncInterval);
                }
            }
            else
            {
                Debug.LogError($"[FirebaseScoreManager] Erreur d'initialisation Firebase: {dependencyStatus.Result}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseScoreManager] Exception lors de l'initialisation Firebase: {ex.Message}");
        }
        #else
        Debug.LogWarning("[FirebaseScoreManager] Firebase Database n'est pas disponible. Ajoutez le symbol FIREBASE_DATABASE pour l'activer.");
        isInitialized = false;
        #endif
        
        yield break;
    }
    
    private void HandleWalletAddressChanged(string address)
    {
        if (address == currentWalletAddress)
            return;
            
        currentWalletAddress = address;
        Debug.Log($"[FirebaseScoreManager] Adresse wallet mise à jour: {address}");
        
        if (string.IsNullOrEmpty(address))
        {
            // Wallet déconnecté, réinitialiser le score
            currentScore = 0;
            UpdateScoreDisplay();
        }
        #if FIREBASE_DATABASE
        else if (isInitialized)
        {
            // Charger le score depuis Firebase
            FetchScoreFromFirebase(address);
        }
        #endif
    }
    
    #if FIREBASE_DATABASE
    private void FetchScoreFromFirebase(string address)
    {
        if (string.IsNullOrEmpty(address) || !isInitialized)
            return;
            
        // Nettoyer l'adresse pour une utilisation comme clé dans Firebase (enlever les caractères interdits)
        string safeAddress = MakeSafeFirebaseKey(address);
        
        // Construire le chemin complet
        string path = $"{databasePath}{safeAddress}";
        Debug.Log($"[FirebaseScoreManager] Chargement du score depuis: {path}");
        
        databaseReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseScoreManager] Erreur lors de la récupération du score: {task.Exception}");
                return;
            }
            
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.HasChild(scoreField))
                {
                    // Extraire le score
                    currentScore = Convert.ToInt32(snapshot.Child(scoreField).Value);
                    Debug.Log($"[FirebaseScoreManager] Score chargé: {currentScore}");
                }
                else
                {
                    // Aucun score trouvé, créer un nouveau profil
                    Debug.Log("[FirebaseScoreManager] Aucun score trouvé, création d'un nouveau profil");
                    currentScore = 0;
                    
                    // Si GameManager existe et contient un score, l'utiliser
                    if (GameManager.Instance != null)
                    {
                        currentScore = GameManager.Instance.generalScore;
                    }
                    
                    SaveScoreToFirebase();
                }
                
                // Mettre à jour l'affichage
                UpdateScoreDisplay();
                
                // Mettre à jour le score du GameManager si différent
                if (GameManager.Instance != null && GameManager.Instance.generalScore != currentScore)
                {
                    GameManager.Instance.generalScore = currentScore;
                }
            }
        });
    }
    
    public void SaveScoreToFirebase()
    {
        if (string.IsNullOrEmpty(currentWalletAddress) || !isInitialized)
            return;
        
        // Prendre le score actuel du GameManager si disponible
        if (GameManager.Instance != null)
        {
            currentScore = GameManager.Instance.generalScore;
        }
        
        string safeAddress = MakeSafeFirebaseKey(currentWalletAddress);
        string path = $"{databasePath}{safeAddress}";
        
        // Créer un dictionnaire avec les données à sauvegarder
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { scoreField, currentScore },
            { "lastUpdate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            { "walletAddress", currentWalletAddress }
        };
        
        databaseReference.Child(path).UpdateChildrenAsync(updates).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseScoreManager] Erreur lors de la sauvegarde: {task.Exception}");
            }
            else if (task.IsCompleted)
            {
                Debug.Log($"[FirebaseScoreManager] Score sauvegardé: {currentScore}");
            }
        });
    }
    #endif
    
    private void SyncScoreWithFirebase()
    {
        #if FIREBASE_DATABASE
        if (isInitialized && !string.IsNullOrEmpty(currentWalletAddress))
        {
            // Vérifier si le score a changé
            if (GameManager.Instance != null && GameManager.Instance.generalScore != currentScore)
            {
                currentScore = GameManager.Instance.generalScore;
                SaveScoreToFirebase();
            }
            else
            {
                // Rafraîchir le score depuis Firebase
                FetchScoreFromFirebase(currentWalletAddress);
            }
        }
        #endif
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreDisplay != null)
        {
            scoreDisplay.text = scorePrefix + currentScore.ToString();
        }
    }
    
    private string MakeSafeFirebaseKey(string key)
    {
        // Firebase n'autorise pas certains caractères dans les clés
        return key.Replace(".", "-")
                 .Replace("$", "")
                 .Replace("#", "")
                 .Replace("[", "")
                 .Replace("]", "")
                 .Replace("/", "_");
    }
    
    void OnDestroy()
    {
        // Se désabonner des événements
        WalletManager.OnWalletAddressChanged -= HandleWalletAddressChanged;
        
        // Arrêter la synchronisation automatique
        CancelInvoke(nameof(SyncScoreWithFirebase));
    }
    
    /// <summary>
    /// Force la sauvegarde immédiate du score actuel dans Firebase.
    /// Cette méthode améliorée inclut des vérifications supplémentaires.
    /// </summary>
    public void ForceScoreSave()
    {
        Debug.Log("[FirebaseScoreManager] Sauvegarde forcée du score");
        
        #if FIREBASE_DATABASE
        // Vérifier l'état d'initialisation
        if (!isInitialized)
        {
            Debug.LogWarning("[FirebaseScoreManager] Firebase non initialisé, tentative d'initialisation avant sauvegarde");
            StartCoroutine(InitAndSave());
            return;
        }
        
        // Vérifier l'adresse wallet
        if (string.IsNullOrEmpty(currentWalletAddress))
        {
            // Essayer de récupérer l'adresse depuis WalletManager
            currentWalletAddress = WalletManager.CurrentWalletAddress;
            
            if (string.IsNullOrEmpty(currentWalletAddress))
            {
                Debug.LogWarning("[FirebaseScoreManager] Impossible de sauvegarder: aucune adresse wallet disponible");
                return;
            }
        }
        
        // Récupérer le score depuis plusieurs sources disponibles
        if (GameManager.Instance != null)
        {
            currentScore = GameManager.Instance.generalScore;
            Debug.Log($"[FirebaseScoreManager] Score récupéré du GameManager: {currentScore}");
        }
        else
        {
            // Fallback: essayer de récupérer depuis PlayerPrefs
            int savedScore = PlayerPrefs.GetInt("TempNadsScore", -1);
            if (savedScore >= 0)
            {
                currentScore = savedScore;
                Debug.Log($"[FirebaseScoreManager] Score récupéré de PlayerPrefs: {currentScore}");
            }
        }
        
        // Sauvegarder le score dans Firebase
        SaveScoreToFirebase();
        
        // Mettre à jour l'affichage
        UpdateScoreDisplay();
        #else
        Debug.LogWarning("[FirebaseScoreManager] Firebase Database n'est pas activé. Ajoutez le symbol FIREBASE_DATABASE.");
        #endif
    }
    
    #if FIREBASE_DATABASE
    private IEnumerator InitAndSave()
    {
        // Attendre que l'initialisation soit complète
        yield return InitializeFirebase();
        
        // Essayer de sauvegarder à nouveau
        if (isInitialized)
        {
            Debug.Log("[FirebaseScoreManager] Initialisation réussie, sauvegarde maintenant");
            ForceScoreSave();
        }
    }
    #endif
    
    // Méthode publique pour forcer un rechargement
    public void ForceScoreRefresh()
    {
        #if FIREBASE_DATABASE
        if (isInitialized && !string.IsNullOrEmpty(currentWalletAddress))
        {
            FetchScoreFromFirebase(currentWalletAddress);
        }
        #endif
    }
    
    // Méthode additionnelle pour synchroniser avec Firestore
    public void SyncWithFirestore()
    {
        #if FIREBASE_DATABASE
        if (isInitialized && !string.IsNullOrEmpty(currentWalletAddress))
        {
            // Mise à jour vers Firestore si nécessaire
            var firestoreManager = FindObjectOfType<ScoreManager>();
            if (firestoreManager != null && currentScore > 0)
            {
                // Sauvegarder le score actuel dans Firestore
                Debug.Log("[FirebaseScoreManager] Synchronisation avec Firestore");
                // Cette méthode dépendra de l'implémentation de votre ScoreManager
                // firestoreManager.UpdateScoreWithValue(currentScore);
            }
        }
        #endif
    }
}