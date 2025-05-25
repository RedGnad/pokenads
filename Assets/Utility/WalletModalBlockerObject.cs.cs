using UnityEngine;

public class WalletModalBlockerObject : MonoBehaviour
{
    // Ce script est simplement attaché à un objet qui sert d'indicateur
    // Sa seule présence/activation dans la scène signale que le modal est ouvert

    [Tooltip("Optionnel: Désactiver automatiquement après un certain temps (failsafe)")]
    public float autoDestroyTime = 60f; // Secondes
    
    void Awake()
    {
        // Assurer un nom standard pour faciliter la recherche
        gameObject.name = "WalletModalBlocker";
        
        // Optionnellement, ajouter un failsafe pour éviter de bloquer indéfiniment
        if (autoDestroyTime > 0)
            Destroy(gameObject, autoDestroyTime);
    }
}