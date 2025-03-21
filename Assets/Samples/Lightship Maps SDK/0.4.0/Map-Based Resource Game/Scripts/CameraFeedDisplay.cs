using UnityEngine;
using UnityEngine.UI;

public class CameraFeedDisplay : MonoBehaviour
{
    [SerializeField]
    private RawImage rawImage;

    private WebCamTexture webCamTexture;

    private void Awake()
    {
        if (rawImage != null)
        {
            rawImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CameraFeedDisplay] rawImage non assigné");
        }
    }

    public void StartCameraFeed()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            if (rawImage != null)
            {
                Debug.Log("[CameraFeedDisplay] Réactivation RawImage");
                rawImage.gameObject.SetActive(true);

                Debug.Log("[CameraFeedDisplay] Création WebCamTexture");
                webCamTexture = new WebCamTexture();

                Debug.Log("[CameraFeedDisplay] Textures");
                rawImage.texture = webCamTexture;
                rawImage.material.mainTexture = webCamTexture;

                Debug.Log("[CameraFeedDisplay] Caméra");
                webCamTexture.Play();
            }
            else
            {
                Debug.LogError("[CameraFeedDisplay] pas de rawimage");
            }
        }
        else
        {
            Debug.LogWarning("[CameraFeedDisplay] Aucune webcam détectée sur cet appareil.");
        }
    }
}