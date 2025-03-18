using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    private ARSession arSession;
    private ARSessionOrigin arSessionOrigin;

    void Awake()
    {
        // Trouver les composants AR dans la scène
        arSession = FindObjectOfType<ARSession>();
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();

        if (arSession == null || arSessionOrigin == null)
        {
            Debug.LogError("ARSession ou ARSessionOrigin manquant dans la scène.");
        }
    }

    void Start()
    {
        // Si nécessaire, ajouter la configuration spécifique à la plateforme
        #if UNITY_ANDROID
            // Exemple : configuration ARCore si besoin
        #elif UNITY_IOS
            // Exemple : configuration ARKit si besoin
        #endif
    }
    
    void Update()
    {
        // Vous pouvez ajouter ici la logique de mise-à-jour de la session AR si nécessaire
    }

    /// <summary>
    /// Active ou désactive la session AR.
    /// </summary>
    /// <param name="isActive">True pour activer, false pour désactiver</param>
    public void ToggleARSession(bool isActive)
    {
        if (arSession == null)
        {
            Debug.LogWarning("Impossible de toggler ARSession : arSession est null!");
            return;
        }

        if (isActive)
        {
            // Active et réinitialise la session AR
            Debug.Log("Activation de ARSession");
            arSession.enabled = true;
            arSession.Reset();
        }
        else
        {
            // Désactive la session AR
            Debug.Log("Désactivation de ARSession");
            arSession.enabled = false;
        }
    }
}