using UnityEngine;

public class RabbyWebView : MonoBehaviour
{
    private WebViewObject _webViewObject;
    private bool _isInitialized = false;

    void Start()
    {
        // Ne rien faire ici : on initialise plus tard, au premier clic
    }

    /// <summary>
    /// Appelée depuis votre bouton UI (OnClick) pour ouvrir le WebView.
    /// </summary>
    public void OpenRabbyWebView()
    {
        if (!_isInitialized)
        {
            // Création et initialisation du WebViewObject
            _webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            _webViewObject.Init(
                cb: (msg) =>
                {
                    Debug.Log($"Message from WebView: {msg}");
                    // Traitez ici le message JSON renvoyé depuis rabby-connect.html
                    // Par exemple : récupérer l'adresse du wallet et fermer le WebView
                    var data = JsonUtility.FromJson<ConnectMessage>(msg);
                    if (data.type == "connected")
                    {
                        Debug.Log("Adresse du portefeuille : " + data.account);
                        _webViewObject.SetVisibility(false);
                    }
                },
                err: (err) => Debug.LogError($"[WebView] Error: {err}"),
                ld: (ld) => Debug.Log($"[WebView] Loaded: {ld}"),
                enableWKWebView: true,
                transparent: true
            );
            _webViewObject.SetMargins(0, 0, 0, 0);
            _isInitialized = true;
        }

        // Charger et afficher la page de connexion Rabby
        _webViewObject.LoadURL("https://rabby-connect.web.app/rabby-connect.html");
        _webViewObject.SetVisibility(true);
    }

    [System.Serializable]
    private class ConnectMessage
    {
        public string type;
        public string account;
    }
}
