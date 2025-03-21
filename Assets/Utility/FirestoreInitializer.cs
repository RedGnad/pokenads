using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.Events;

public class FirebaseInitializer : MonoBehaviour
{
    // Instance statique pour accéder à Firestore dans d'autres scripts.
    public static FirebaseFirestore FirestoreDb;

    // Événement qui sera invoqué une fois Firebase prêt.
    public UnityEvent OnFirebaseReady;

    void Awake()
    {
        // Empêche la destruction de cet objet entre les scènes.
        DontDestroyOnLoad(this.gameObject);
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        // Vérifier et corriger les dépendances avant d’appeler d’autres fonctions Firebase.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase est prêt");
                // Instanciation de Firestore
                FirestoreDb = FirebaseFirestore.DefaultInstance;

                // Notifier que Firebase est prêt
                if (OnFirebaseReady != null)
                {
                    OnFirebaseReady.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Impossible de résoudre les dépendances Firebase : {dependencyStatus}");
            }
        });
    }
}
