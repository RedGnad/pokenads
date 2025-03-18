using UnityEngine;
using UnityEngine.SceneManagement;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance;

        [SerializeField]
        private GameObject secondScreenPanel;

        private void Awake()
        {
            Instance = this;
        }

        public void ShowSecondScreen()
        {
            if (ARCompatibilityChecker.IsARCoreSupported)
            {
                SceneTransitionData.LoadingTime = 1f;
                SceneManager.LoadScene("CameraFeedScene");
            }
            else
            {
                Debug.Log("ARCore non supporté. Redirection vers l'écran sans AR.");
                SceneTransitionData.LoadingTime = 1f;
                SceneManager.LoadScene("NonCameraFeedScene"); // Remplacez par le nom de votre scène sans AR
            }
        }

        public void ReturnToMap()
        {
            Debug.Log("ReturnToMap appelée");
            SceneTransitionData.LoadingTime = 1f;
            SceneManager.LoadScene("MapScreen");
        }

        public void HideSecondScreen()
        {
            if (secondScreenPanel != null)
            {
                secondScreenPanel.SetActive(false);
            }
        }
    }
}
