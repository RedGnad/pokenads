using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    public class FeatureInteraction : MonoBehaviour
    {
        public float maxInteractionDistance = 30f;
        private Transform playerTransform;
        
        // Vérifier périodiquement l'existence de l'objet bloqueur
        private bool isBlocked = false;
        private float lastCheckTime = 0f;
        private const float CHECK_INTERVAL = 0.5f; // Vérifier toutes les 0.5 secondes
    
        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Aucun joueur trouvé avec le tag 'Player'.");
            }
        }
    
        void Update()
        {
            // Vérifier périodiquement si l'objet bloqueur existe
            if (Time.time > lastCheckTime + CHECK_INTERVAL)
            {
                lastCheckTime = Time.time;
                isBlocked = GameObject.Find("WalletModalBlocker") != null;
            }
        }
    
        private void OnMouseDown()
        {
            // Vérifier si l'inventaire est ouvert ou si le modal wallet est ouvert
            if (InventoryUI.IsInventoryOpen || isBlocked)
            {
                Debug.Log("[FeatureInteraction] Interaction ignorée - UI modale ouverte");
                return;
            }
            
            if (playerTransform == null)
                return;
    
            float distance = Vector3.Distance(playerTransform.position, transform.position);
    
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowText("too far", transform.position + Vector3.up * 2f);
            }
        }
    }
}