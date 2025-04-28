using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private const string TEMP_SCORE_KEY = "TempNadsScore";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// À appeler quand le combat est terminé pour attribuer 20 points.
    /// </summary>
    public void OnCombatFinished()
    {
        // Écrase la valeur précédente au lieu de cumuler
        PlayerPrefs.SetInt(TEMP_SCORE_KEY, 20);
        PlayerPrefs.Save();

        // Lance la mise à jour Firestore
        UpdateFirestoreScore(20);
    }

    private void UpdateFirestoreScore(int pointsToAdd)
    {
        string addr = WalletManager.CurrentWalletAddress;
        if (string.IsNullOrEmpty(addr)) return;

        var db = FirebaseFirestore.DefaultInstance;
        var docRef = db.Collection("Scores").Document(addr);

        // <<< CHANGEMENT MINIMAL >>>
        // Utiliser UpdateAsync au lieu de Update
        docRef.UpdateAsync("Score", FieldValue.Increment(pointsToAdd))
              .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                // Suppression du TempNadsScore dès que Firestore a pris en compte l'incrément
                PlayerPrefs.DeleteKey(TEMP_SCORE_KEY);
                PlayerPrefs.Save();
                Debug.Log("[ScoreManager] Firestore mis à jour +20, TempNadsScore purgé");
            }
            else
            {
                Debug.LogWarning("[ScoreManager] Échec update Firestore : " + task.Exception);
            }
        });
    }
}