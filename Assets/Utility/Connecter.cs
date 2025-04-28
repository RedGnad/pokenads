using UnityEngine;
using System;
using Reown.AppKit.Unity;

public class ConnectWalletButton : MonoBehaviour
{
    // Propriétés et événements statiques pour être accessibles de partout
    public static string CurrentWalletAddress { get; private set; } = "";
    public static event Action OnWalletConnected;
    public static event Action OnWalletDisconnected;
    
    private void Start()
    {
        // S'abonner aux événements d'AppKit
        try
        {
            var appKit = typeof(AppKit).GetProperty("Instance")?.GetValue(null);
            
            if (appKit != null)
            {
                // Les événements ont été renommés dans AppKit 1.3.1
                // "OnWalletConnected" → "AccountConnected"
                var connectedEventInfo = appKit.GetType().GetEvent("AccountConnected");
                if (connectedEventInfo != null)
                {
                    var addMethod = connectedEventInfo.GetAddMethod();
                    var delegateType = connectedEventInfo.EventHandlerType;
                    var handler = Delegate.CreateDelegate(delegateType, this, 
                        typeof(ConnectWalletButton).GetMethod("HandleAccountConnected", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                    addMethod.Invoke(appKit, new object[] { handler });
                    Debug.Log("ConnectWalletButton: Abonnement à AccountConnected réussi");
                }
                else
                {
                    Debug.LogError("ConnectWalletButton: Événement AccountConnected non trouvé");
                }
                
                // "OnWalletDisconnected" → "AccountDisconnected"
                var disconnectedEventInfo = appKit.GetType().GetEvent("AccountDisconnected");
                if (disconnectedEventInfo != null)
                {
                    var addMethod = disconnectedEventInfo.GetAddMethod();
                    var delegateType = disconnectedEventInfo.EventHandlerType;
                    var handler = Delegate.CreateDelegate(delegateType, this, 
                        typeof(ConnectWalletButton).GetMethod("HandleAccountDisconnected", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
                    addMethod.Invoke(appKit, new object[] { handler });
                    Debug.Log("ConnectWalletButton: Abonnement à AccountDisconnected réussi");
                }
                
                // Vérifier la connexion initiale avec GetAccountAsync
                var methodInfo = appKit.GetType().GetMethod("GetAccountAsync");
                if (methodInfo != null)
                {
                    var task = methodInfo.Invoke(appKit, null);
                    // Utiliser IsAccountConnected pour vérifier si un compte est connecté
                    var isConnectedProp = appKit.GetType().GetProperty("IsAccountConnected");
                    if (isConnectedProp != null && (bool)isConnectedProp.GetValue(appKit))
                    {
                        Debug.Log("ConnectWalletButton: Compte déjà connecté, récupération de l'adresse");
                        StartCoroutine(GetAddressFromTask(task));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ConnectWalletButton: Erreur lors de l'initialisation: {ex.Message}");
        }
    }
    
    private System.Collections.IEnumerator GetAddressFromTask(object task)
    {
        if (task == null) yield break;
        
        // Attendre que la tâche soit complétée
        var isCompletedProp = task.GetType().GetProperty("IsCompleted");
        if (isCompletedProp == null) yield break;
        
        while (!(bool)isCompletedProp.GetValue(task))
        {
            yield return null;
        }
        
        var isFaultedProp = task.GetType().GetProperty("IsFaulted");
        if (isFaultedProp == null || (bool)isFaultedProp.GetValue(task)) yield break;
        
        var resultProp = task.GetType().GetProperty("Result");
        if (resultProp == null) yield break;
        
        var account = resultProp.GetValue(task);
        if (account == null) yield break;
        
        var addressProp = account.GetType().GetProperty("Address");
        if (addressProp == null) yield break;
        
        string address = addressProp.GetValue(account) as string;
        if (!string.IsNullOrEmpty(address))
        {
            CurrentWalletAddress = address;
            OnWalletConnected?.Invoke();
        }
    }
    
    // Méthode pour gérer AccountConnected (nouveau format avec Event Args)
    private void HandleAccountConnected(object sender, EventArgs e)
    {
        Debug.Log("ConnectWalletButton: AccountConnected reçu, récupération de l'adresse");
        
        try
        {
            var appKit = typeof(AppKit).GetProperty("Instance")?.GetValue(null);
            if (appKit != null)
            {
                var getAccountMethod = appKit.GetType().GetMethod("GetAccountAsync");
                if (getAccountMethod != null)
                {
                    var task = getAccountMethod.Invoke(appKit, null);
                    StartCoroutine(GetAddressFromTask(task));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ConnectWalletButton: Erreur lors de la récupération de l'adresse: {ex.Message}");
        }
    }
    
    // Méthode pour gérer AccountDisconnected (nouveau format avec Event Args)
    private void HandleAccountDisconnected(object sender, EventArgs e)
    {
        Debug.Log("ConnectWalletButton: AccountDisconnected reçu");
        CurrentWalletAddress = "";
        OnWalletDisconnected?.Invoke();
    }
}