using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;
using Reown.AppKit.Unity;            // AppKitCore
using Reown.AppKit.Unity.Components;  // ConnectorController

[RequireComponent(typeof(UnityButton))]
public class DisconnectWalletButton : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private bool requireConfirmation = true;

    private UnityButton _btn;

    void Awake()
    {
        Debug.Log("[Disconnect] Awake()");
    }

    void OnEnable()
    {
        Debug.Log("[Disconnect] OnEnable()");
    }

    void Start()
    {
        Debug.Log("[Disconnect] Start()");
        _btn = GetComponent<UnityButton>();
        if (_btn == null)
            Debug.LogError("[Disconnect] ⛔ UnityButton introuvable sur le GameObject");
        else
            Debug.Log("[Disconnect] UnityButton trouvé");

        _btn?.onClick.AddListener(HandleDisconnectClicked);
        Debug.Log("[Disconnect] Listener UI ajouté");

        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        WalletManager.OnWalletAddressChanged += UpdateButtonVisibility;
        Debug.Log("[Disconnect] Abonné à OnWalletAddressChanged");

        UpdateButtonVisibility(WalletManager.CurrentWalletAddress);
        Debug.Log("[Disconnect] UpdateButtonVisibility appelé; adresse = “" 
                  + WalletManager.CurrentWalletAddress + "”");

        Debug.Log("[Disconnect] Lancement de DelayedSubscribeToNative()");
        StartCoroutine(DelayedSubscribeToNative());
    }

    IEnumerator DelayedSubscribeToNative()
    {
        Debug.Log("[Disconnect] Coroutine démarrée, attente de ConnectorController…");
        int tick = 0;
        while (AppKit.ConnectorController == null)
        {
            if (tick % 60 == 0)
                Debug.Log($"[Disconnect] Frame {tick}: ConnectorController toujours null");
            tick++;
            yield return null;
        }

        Debug.Log("[Disconnect] ConnectorController prêt !");
        AppKit.ConnectorController.AccountDisconnected += OnNativeDisconnected;
        Debug.Log("[Disconnect] Abonné à AccountDisconnected");
    }

    void UpdateButtonVisibility(string addr)
    {
        bool active = !string.IsNullOrEmpty(addr);
        gameObject.SetActive(active);
        Debug.Log("[Disconnect] UpdateButtonVisibility → active=" + active);
    }

    void HandleDisconnectClicked()
    {
        Debug.Log("[Disconnect] HandleDisconnectClicked()");
        if (requireConfirmation && confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            Debug.Log("[Disconnect] confirmationPanel activé");
        }
        else
        {
            DoDisconnect();
        }
    }

    public void CancelDisconnect()
    {
        Debug.Log("[Disconnect] CancelDisconnect()");
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    void DoDisconnect()
    {
        Debug.Log("[Disconnect] DoDisconnect()");
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.Disconnect();
            Debug.Log("[Disconnect] WalletManager.Disconnect() appelé");
        }
        else
        {
            Debug.LogWarning("[Disconnect] WalletManager introuvable");
        }
    }

    private void OnNativeDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
    {
        Debug.Log("[Disconnect] OnNativeDisconnected() event reçu");
        DoDisconnect();
    }

    void OnDestroy()
    {
        Debug.Log("[Disconnect] OnDestroy()");
        if (_btn != null)
        {
            _btn.onClick.RemoveListener(HandleDisconnectClicked);
            Debug.Log("[Disconnect] Listener UI retiré");
        }

        WalletManager.OnWalletAddressChanged -= UpdateButtonVisibility;
        Debug.Log("[Disconnect] Désabonné de OnWalletAddressChanged");

        if (AppKit.ConnectorController != null)
        {
            AppKit.ConnectorController.AccountDisconnected -= OnNativeDisconnected;
            Debug.Log("[Disconnect] Désabonné de AccountDisconnected");
        }
    }
}