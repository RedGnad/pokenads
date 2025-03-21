using UnityEngine;
using UnityEngine.UI;

public class FirebaseButtonEnabler : MonoBehaviour
{
    // Référence au bouton qui déclenche la mise à jour du score.
    public Button updateScoreButton;

    void Start()
    {
        // Désactiver le bouton tant que Firebase n'est pas prêt.
        if (updateScoreButton != null)
        {
            updateScoreButton.interactable = false;
        }

        // Chercher l'initialisateur Firebase dans la scène.
        FirebaseInitializer initializer = FindObjectOfType<FirebaseInitializer>();
        if (initializer != null)
        {
            initializer.OnFirebaseReady.AddListener(EnableButton);
        }
    }

    void EnableButton()
    {
        if (updateScoreButton != null)
        {
            updateScoreButton.interactable = true;
            Debug.Log("Bouton activé : Firebase est initialisé.");
        }
    }
}
