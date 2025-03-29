using UnityEngine;
using TMPro;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    internal class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance;

        [SerializeField]
        private GameObject floatingTextPrefab;

        [SerializeField]
        private GameObject tooFarTextPrefab;
        
        [SerializeField]
        private bool enableFloatingText = true;
        
        private void Awake()
        {
            Instance = this;
        }

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