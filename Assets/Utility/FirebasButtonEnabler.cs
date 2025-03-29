using UnityEngine;
using UnityEngine.UI;

public class FirebaseButtonEnabler : MonoBehaviour
{
    public Button updateScoreButton;

    void Start()
    {
        if (updateScoreButton != null)
        {
            updateScoreButton.interactable = false;
        }

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
