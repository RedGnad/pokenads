namespace MyGame.ReactionScreen
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class LoadingScreenManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject loadingScreenPanel; // Panneau de chargement assigné dans l'inspecteur

        [SerializeField]
        private float defaultLoadingTime = 3f;

        private float currentLoadingTime;

        void Awake()
        {
            currentLoadingTime = SceneTransitionData.LoadingTime;
        }

        void Start()
        {
            if (loadingScreenPanel != null)
            {
                loadingScreenPanel.SetActive(true);
                StartCoroutine(HideLoadingScreen());
            }
        }

        private IEnumerator HideLoadingScreen()
        {
            yield return new WaitForSeconds(currentLoadingTime);
            loadingScreenPanel.SetActive(false);
            
            SceneTransitionData.LoadingTime = defaultLoadingTime;
        }
    }
}