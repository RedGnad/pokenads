using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using ChainSafe.Gaming.UnityPackage; // Pour accéder à Web3Unity

public class ScoreManager : MonoBehaviour
{
    // Méthode appelée par le bouton pour mettre à jour le score.
    public void UpdatePlayerScore()
    {
        // Vérifier que Firestore est prêt.
        if (FirebaseInitializer.FirestoreDb == null)
        {
            Debug.LogWarning("Firebase Firestore n'est pas encore initialisé.");
            return;
        }

        // Récupérer l'adresse du wallet via Web3Unity.
        string walletAddress = Web3Unity.Instance?.PublicAddress;
        if (string.IsNullOrEmpty(walletAddress))
        {
            Debug.LogWarning("Wallet non connecté");
            return;
        }

        // Utiliser l'adresse du wallet comme ID de document dans la collection "Scores".
        DocumentReference docRef = FirebaseInitializer.FirestoreDb.Collection("Scores").Document(walletAddress);

        // Exécuter une transaction pour mettre à jour le score de manière atomique.
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

                // Incrémente le score de 20 points.
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
