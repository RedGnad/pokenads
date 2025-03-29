using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using ChainSafe.Gaming.UnityPackage;

public class ScoreManager : MonoBehaviour
{
    public void UpdatePlayerScore()
    {
        if (FirebaseInitializer.FirestoreDb == null)
        {
            Debug.LogWarning("Firebase Firestore n'est pas encore initialisé.");
            return;
        }

        string walletAddress = Web3Unity.Instance?.PublicAddress;
        if (string.IsNullOrEmpty(walletAddress))
        {
            Debug.LogWarning("Wallet non connecté");
            return;
        }

        DocumentReference docRef = FirebaseInitializer.FirestoreDb.Collection("Scores").Document(walletAddress);

        FirebaseInitializer.FirestoreDb.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(docRef).ContinueWithOnMainThread(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                int currentScore = 0;
                if (snapshot.Exists && snapshot.ContainsField("Score"))
                {
                    currentScore = snapshot.GetValue<int>("Score");
                }

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "User", walletAddress },
                    { "Score", currentScore + 20 }
                };

                transaction.Set(docRef, updates, SetOptions.MergeAll);
                return true;
            });
        }).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Erreur lors de la mise à jour du score : " + task.Exception);
            }
            else
            {
                Debug.Log("Score mis à jour avec succès.");
            }
        });
    }
}
