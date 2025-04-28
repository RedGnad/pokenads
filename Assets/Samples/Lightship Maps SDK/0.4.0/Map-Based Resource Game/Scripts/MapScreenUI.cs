using UnityEngine;
using TMPro;
using System.Reflection;

public class MapGameUI : MonoBehaviour
{
    public TextMeshProUGUI generalScoreText;
    public TextMeshProUGUI walletAddressText;
    
    // Pour le débogage
    private bool loggedThisSession = false;

    void OnEnable()
    {
        RefreshScore();
        RefreshWalletAddress();
        
        // Réinitialiser le flag pour le logging
        loggedThisSession = false;
    }
    
    void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            RefreshWalletAddress();
        }
    }

    void RefreshScore()
    {
        if (generalScoreText != null && GameManager.Instance != null)
        {
            generalScoreText.text = "Nads : " + GameManager.Instance.generalScore;
        }
    }
    
    void RefreshWalletAddress()
    {
        if (walletAddressText != null)
        {
            Debug.Log("MapScreenUI: Tentative de récupération de l'adresse wallet");
            string walletAddress = GetWalletAddress();
            
            if (!string.IsNullOrEmpty(walletAddress))
            {
                Debug.Log($"MapScreenUI: Adresse trouvée: {walletAddress}");
                if (walletAddress.Length > 16)
                {
                    walletAddressText.text = walletAddress.Substring(0, 6) + "..." + 
                                            walletAddress.Substring(walletAddress.Length - 4);
                }
                else
                {
                    walletAddressText.text = walletAddress;
                }
            }
            else
            {
                Debug.Log("MapScreenUI: Aucune adresse trouvée");
                walletAddressText.text = "Not connected";
                
                // Log détaillé une seule fois par session
                if (!loggedThisSession)
                {
                    LogWalletComponents();
                    loggedThisSession = true;
                }
            }
        }
    }

    private void LogWalletComponents()
    {
        // Rechercher les composants liés au wallet pour debug
        Debug.Log("======= RECHERCHE DES COMPOSANTS WALLET =======");
        
        // Chercher ConnectWalletButton
        var connectWalletObjs = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        foreach(var obj in connectWalletObjs)
        {
            if (obj.GetType().Name.Contains("ConnectWalletButton") || 
                obj.GetType().Name.Contains("Wallet") ||
                obj.GetType().Name.Contains("AppKit"))
            {
                Debug.Log($"Trouvé: {obj.name} - Type: {obj.GetType().FullName}");
            }
        }
    }

    private string GetWalletAddress()
    {
        // Méthode 1: Via ConnectWalletButton (qui semble être utilisé selon les logs)
        try
        {
            // Trouver tous les types qui pourraient être ConnectWalletButton
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try 
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name == "ConnectWalletButton")
                        {
                            // Vérifier s'il a une propriété CurrentWalletAddress
                            var prop = type.GetProperty("CurrentWalletAddress", 
                                        BindingFlags.Public | BindingFlags.Static);
                                        
                            if (prop != null)
                            {
                                string address = (string)prop.GetValue(null);
                                if (!string.IsNullOrEmpty(address))
                                {
                                    Debug.Log($"MapScreenUI: Adresse trouvée via ConnectWalletButton: {address}");
                                    return address;
                                }
                            }
                        }
                    }
                }
                catch { /* Ignorer les erreurs par assembly */ }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Erreur recherche ConnectWalletButton: {ex.Message}");
        }
        
        // Méthode 2: Via WalletConnectionHandler (mentionné dans les logs)
        try
        {
            var connectionHandler = FindObjectOfType<MonoBehaviour>();
            if (connectionHandler != null && connectionHandler.GetType().Name == "WalletConnectionHandler")
            {
                var method = connectionHandler.GetType().GetMethod("GetWalletAddress");
                if (method != null)
                {
                    string address = (string)method.Invoke(connectionHandler, null);
                    if (!string.IsNullOrEmpty(address))
                    {
                        Debug.Log($"MapScreenUI: Adresse trouvée via WalletConnectionHandler: {address}");
                        return address;
                    }
                }
                
                // Essayer de trouver un champ qui contient l'adresse
                var fields = connectionHandler.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.Name.ToLower().Contains("address") || field.Name.ToLower().Contains("wallet"))
                    {
                        var value = field.GetValue(connectionHandler);
                        if (value is string address && !string.IsNullOrEmpty(address))
                        {
                            Debug.Log($"MapScreenUI: Adresse trouvée via champ {field.Name}: {address}");
                            return address;
                        }
                    }
                }
            }
        }
        catch { /* Ignorer les erreurs */ }
        
        // Option 3: Via AppKit directement (code original)
        try
        {
            var appKitType = System.Type.GetType("Reown.AppKit.Unity.AppKit, Reown.AppKit.Unity") ?? 
                            System.Type.GetType("AppKit") ?? 
                            System.Type.GetType("Reown.AppKit.Unity.AppKit");
            
            if (appKitType != null)
            {
                var instanceProp = appKitType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    if (instance != null)
                    {
                        // Essayons différentes propriétés
                        string[] walletProps = new string[] { "Wallet", "wallet", "CurrentWallet", "currentWallet" };
                        foreach (string propName in walletProps)
                        {
                            var walletProp = instance.GetType().GetProperty(propName);
                            if (walletProp != null)
                            {
                                var wallet = walletProp.GetValue(instance);
                                if (wallet != null)
                                {
                                    string[] addressProps = new string[] { "Address", "address", "WalletAddress", "walletAddress" };
                                    foreach (string addrProp in addressProps)
                                    {
                                        var addressProp = wallet.GetType().GetProperty(addrProp);
                                        if (addressProp != null)
                                        {
                                            string address = (string)addressProp.GetValue(wallet);
                                            if (!string.IsNullOrEmpty(address))
                                            {
                                                Debug.Log($"MapScreenUI: Adresse trouvée via AppKit.{propName}.{addrProp}: {address}");
                                                return address;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Essayons les méthodes directement
                        var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var method in methods)
                        {
                            if ((method.Name.Contains("GetWallet") || method.Name.Contains("GetAddress")) && 
                                method.GetParameters().Length == 0 && 
                                method.ReturnType == typeof(string))
                            {
                                string address = (string)method.Invoke(instance, null);
                                if (!string.IsNullOrEmpty(address))
                                {
                                    Debug.Log($"MapScreenUI: Adresse trouvée via AppKit.{method.Name}(): {address}");
                                    return address;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Erreur lors de l'accès à l'adresse AppKit: {ex.Message}");
        }

        // Pas d'adresse trouvée
        return string.Empty;
    }

    public void OnReturnToMapScreen()
    {
        RefreshScore();
        RefreshWalletAddress();
    }
}