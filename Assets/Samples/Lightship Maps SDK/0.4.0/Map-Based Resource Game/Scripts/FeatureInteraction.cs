using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    public class FeatureInteraction : MonoBehaviour
    {
        public float maxInteractionDistance = 30f;
        private Transform playerTransform;

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

        private void OnMouseDown()
        {
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