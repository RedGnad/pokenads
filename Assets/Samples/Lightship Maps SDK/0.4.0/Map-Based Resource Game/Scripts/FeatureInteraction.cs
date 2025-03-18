using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    public class FeatureInteraction : MonoBehaviour
    {
        // Distance maximale d'interaction (ne sera plus utilisée ici pour la logique de gain, mais peut servir à ajuster l'affichage)
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

            // Vous pouvez conserver la vérification de distance pour éventuellement ajuster le comportement,
            // ici on affiche toujours "too far", qu'importe la distance
            float distance = Vector3.Distance(playerTransform.position, transform.position);

            // Optionnel : vous pouvez afficher "too far" uniquement si la distance dépasse un seuil.
            // Dans cet exemple, nous l'affichons toujours.
            if (FloatingTextManager.Instance != null)
            {
                // Afficher "too far" au-dessus de la feature ; le décalage vertical est fixé à 2 unités.
                FloatingTextManager.Instance.ShowText("too far", transform.position + Vector3.up * 2f);
            }
        }
    }
}