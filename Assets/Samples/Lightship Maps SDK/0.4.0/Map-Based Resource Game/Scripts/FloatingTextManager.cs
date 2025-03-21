using UnityEngine;
using TMPro;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    internal class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance;

        [SerializeField]
        private GameObject floatingTextPrefab;

        // Optionnel : un prefab spécifique pour "too far"
        [SerializeField]
        private GameObject tooFarTextPrefab;
        
        [SerializeField]
        private bool enableFloatingText = true;
        
        private void Awake()
        {
            Instance = this;
        }

        // Affiche 'text' à la position indiquée et détruit le cloné après 2 secondes
        public void ShowText(string text, Vector3 position)
        {
            if (!enableFloatingText)
                return;
            
            GameObject prefabToInstantiate = floatingTextPrefab;
            if (text == "too far" && tooFarTextPrefab != null)
            {
                prefabToInstantiate = tooFarTextPrefab;
            }

            if (prefabToInstantiate == null)
                return;
            
            GameObject instance = Instantiate(prefabToInstantiate, position, Quaternion.identity);
            TMP_Text tmp = instance.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = text;
            }
            Destroy(instance, 2f);
        }
    }
}