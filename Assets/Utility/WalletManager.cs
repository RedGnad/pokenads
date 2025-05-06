using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WalletManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TextMeshProUGUI addressDisplay;
    [SerializeField] private float refreshInterval = 1f;
    [SerializeField] private string notConnectedText = "Non connecté";
    [SerializeField] private bool formatAddress = true;
    [SerializeField] private string addressTextObjectName = "WalletAddressText";

    public static string CurrentWalletAddress { get; private set; } = string.Empty;
    public static event Action<string> OnWalletAddressChanged;
    private const string SAVED_WALLET_ADDRESS_KEY = "SavedWalletAddress";

    private string previousAddress = string.Empty;
    private bool initialCheckDone = false;
    private bool isSubscribedToSceneEvents = false;
    public static WalletManager Instance { get; private set; }

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Récupération du TextMeshPro si non assigné
        if (addressDisplay == null)
        {
            addressDisplay = GetComponent<TextMeshProUGUI>();
            if (addressDisplay == null)
            {
                var go = GameObject.Find(addressTextObjectName);
                if (go != null)
                    addressDisplay = go.GetComponent<TextMeshProUGUI>();
            }
        }

        // Chargement de l'adresse sauvegardée
        string saved = PlayerPrefs.GetString(SAVED_WALLET_ADDRESS_KEY, "");
        if (!string.IsNullOrEmpty(saved))
        {
            CurrentWalletAddress = saved;
            previousAddress = saved;
            if (addressDisplay != null)
                addressDisplay.text = formatAddress ? FormatAddress(saved) : saved;
            OnWalletAddressChanged?.Invoke(saved);
        }
        else if (addressDisplay != null)
        {
            addressDisplay.text = notConnectedText;
        }

        // Événement de changement de scène
        if (!isSubscribedToSceneEvents)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            isSubscribedToSceneEvents = true;
        }

        // Démarrage différé de la vérification d'adresse
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f);
        InvokeRepeating(nameof(CheckWalletAddress), 0.1f, refreshInterval);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindAddressTextAfterDelay(0.5f));
    }

    private IEnumerator FindAddressTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (addressDisplay == null || addressDisplay.text == "New Text")
        {
            var go = GameObject.Find(addressTextObjectName);
            if (go != null)
            {
                addressDisplay = go.GetComponent<TextMeshProUGUI>();
                if (addressDisplay != null)
                {
                    addressDisplay.text =
                        !string.IsNullOrEmpty(CurrentWalletAddress)
                            ? (formatAddress ? FormatAddress(CurrentWalletAddress) : CurrentWalletAddress)
                            : notConnectedText;
                }
            }
        }
    }

    void CheckWalletAddress()
    {
        StartCoroutine(GetAddressSafely());
    }

    private IEnumerator GetAddressSafely()
    {
        string newAddress = string.Empty;
        var appKit = FindType("Reown.AppKit.Unity.AppKit");
        if (appKit == null) yield break;

        var prop = appKit.GetProperty("IsConnectionEstablished") ?? appKit.GetProperty("IsConnected");
        if (prop != null && !(bool)prop.GetValue(null, null))
            yield break;

        var method = appKit.GetMethod("GetAccountAsync");
        if (method == null) yield break;

        object task = null;
        try { task = method.Invoke(null, null); }
        catch { yield break; }
        if (task == null) yield break;

        for (int i = 0; i < 10; i++)
        {
            var doneProp = task.GetType().GetProperty("IsCompleted");
            if (doneProp != null && (bool)doneProp.GetValue(task))
            {
                var faulted = task.GetType().GetProperty("IsFaulted")?.GetValue(task);
                if (faulted is bool f && f) break;

                var result = task.GetType().GetProperty("Result")?.GetValue(task);
                if (result != null)
                {
                    var addrProp = result.GetType().GetProperty("Address");
                    newAddress = addrProp?.GetValue(result) as string;
                    if (!string.IsNullOrEmpty(newAddress)) break;
                }
            }
            yield return new WaitForSeconds(0.05f);
        }

        if (newAddress != previousAddress || !initialCheckDone)
        {
            previousAddress = newAddress;
            if (!string.IsNullOrEmpty(newAddress))
            {
                PlayerPrefs.SetString(SAVED_WALLET_ADDRESS_KEY, newAddress);
                PlayerPrefs.Save();
            }

            CurrentWalletAddress = newAddress;
            initialCheckDone = true;

            if (addressDisplay != null)
            {
                addressDisplay.text =
                    !string.IsNullOrEmpty(newAddress)
                        ? (formatAddress ? FormatAddress(newAddress) : newAddress)
                        : notConnectedText;
            }

            OnWalletAddressChanged?.Invoke(newAddress);
        }
    }

    private Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName);
            if (t != null) return t;
        }
        return null;
    }

    private string FormatAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 10)
            return address;
        return address.Substring(0, 6) + "..." + address.Substring(address.Length - 4);
    }

    /// <summary>
    /// Méthode publique de déconnexion : purge l'adresse et les scores.
    /// </summary>
    public void Disconnect()
    {
        // purge adresse
        PlayerPrefs.DeleteKey(SAVED_WALLET_ADDRESS_KEY);
        // purge scores éventuels
        PlayerPrefs.DeleteKey("TempNadsScore");
        PlayerPrefs.DeleteKey("FirestoreScore");
        PlayerPrefs.DeleteKey("CurrentNadsDisplay");
        PlayerPrefs.Save();

        // réinitialiser
        previousAddress = string.Empty;
        CurrentWalletAddress = string.Empty;

        // update UI
        if (addressDisplay != null)
            addressDisplay.text = notConnectedText;

        OnWalletAddressChanged?.Invoke(string.Empty);
        Debug.Log("[WalletManager] Déconnecté et prefs purgés");
    }

    /// <summary>
    /// Ajouté : setter manual pour AppKitInit / RabbyConnector.
    /// </summary>
    public void SetWalletAddress(string address)
    {
        // persistance
        if (string.IsNullOrEmpty(address))
            PlayerPrefs.DeleteKey(SAVED_WALLET_ADDRESS_KEY);
        else
            PlayerPrefs.SetString(SAVED_WALLET_ADDRESS_KEY, address);
        PlayerPrefs.Save();

        // état interne
        previousAddress = address;
        CurrentWalletAddress = address;

        // update UI
        if (addressDisplay != null)
            addressDisplay.text = 
                string.IsNullOrEmpty(address)
                    ? notConnectedText
                    : (formatAddress ? FormatAddress(address) : address);

        // notifier
        OnWalletAddressChanged?.Invoke(address);
        Debug.Log($"[WalletManager] SetWalletAddress → {address}");
    }

    /// <summary>
    /// Force une actualisation immédiate (utilisable si besoin).
    /// </summary>
    public void RefreshWalletAddress()
    {
        StartCoroutine(GetAddressSafely());
        if (addressDisplay != null)
        {
            addressDisplay.text =
                !string.IsNullOrEmpty(CurrentWalletAddress)
                    ? (formatAddress ? FormatAddress(CurrentWalletAddress) : CurrentWalletAddress)
                    : notConnectedText;
        }
    }

    void OnDestroy()
    {
        if (isSubscribedToSceneEvents)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}