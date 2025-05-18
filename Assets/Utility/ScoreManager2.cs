using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private const string TEMP_SCORE_KEY = "TempNadsScore";
    private const string MONSTER_TYPE_KEY = "CapturedMonsterType";

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
    /// À appeler quand le combat est terminé pour attribuer des points.
    /// </summary>
    public void OnCombatFinished()
    {
        // Récupérer le type de monstre s'il a été sauvegardé
        string monsterType = PlayerPrefs.GetString(MONSTER_TYPE_KEY, "unknown");
        
        // Déterminer les points selon le type de monstre
        int points = (monsterType == "Moyaki") ? 30 : 20;
        
        // Si TempNadsScore existe déjà, utiliser cette valeur
        if (PlayerPrefs.HasKey(TEMP_SCORE_KEY))
        {
            points = PlayerPrefs.GetInt(TEMP_SCORE_KEY, 20);
        }
        
        // Écrase la valeur précédente au lieu de cumuler
        PlayerPrefs.SetInt(TEMP_SCORE_KEY, points);
        PlayerPrefs.Save();

        // Lance la mise à jour Firestore avec la bonne valeur de points
        UpdateFirestoreScore(points);
        
        Debug.Log($"[ScoreManager] Combat terminé avec {monsterType}, {points} points attribués");
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
                Debug.Log($"[ScoreManager] Firestore mis à jour +{pointsToAdd}, TempNadsScore purgé");
            }
            else
            {
                Debug.LogWarning("[ScoreManager] Échec update Firestore : " + task.Exception);
            }
        });
    }
}