using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.Events;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseFirestore FirestoreDb;
    public static FirebaseAuth Auth;
    public static string IdToken = "";

    public UnityEvent OnFirebaseReady;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase est prêt");
                FirestoreDb = FirebaseFirestore.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                SignInAnonymously(); 
            }
            else
            {
                Debug.LogError($"Impossible de résoudre les dépendances Firebase : {dependencyStatus}");
            }
        });
    }

    void SignInAnonymously()
    {
        Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Échec de l'authentification anonyme : " + task.Exception);
            }
            else
            {
                Debug.Log("Connecté en mode anonyme");
                task.Result.User.TokenAsync(true).ContinueWithOnMainThread(tokenTask =>
                {
                    if (tokenTask.IsCompleted && !tokenTask.IsFaulted)
                    {
                        IdToken = tokenTask.Result;
                        Debug.Log("ID Token récupéré : " + IdToken);
                        if (OnFirebaseReady != null)
                        {
                            OnFirebaseReady.Invoke();
                        }
                    }
                    else
                    {
                        Debug.LogError("Échec de récupération du token : " + tokenTask.Exception);
                    }
                });
            }
        });
    }
}