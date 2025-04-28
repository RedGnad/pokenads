/*// Assets/Utility/AppKitLogoutListener.cs
using UnityEngine;
using System;
using System.Reflection;

[DefaultExecutionOrder(-100)]
public class AppKitLogoutListener : MonoBehaviour
{
    void Start()
    {
        var inst = typeof(Reown.AppKit.Unity.AppKit)
                   .GetProperty("Instance", BindingFlags.Public|BindingFlags.Static)
                   .GetValue(null);
        if (inst == null) return;

        // Cherche et abonne l'event qui signale la déconnexion native
        var evt = inst.GetType().GetEvent("OnSessionDeleted", BindingFlags.Public|BindingFlags.Instance)
               ?? inst.GetType().GetEvent("OnDisconnected", BindingFlags.Public|BindingFlags.Instance);
        if (evt != null)
        {
            var handler = Delegate.CreateDelegate(evt.EventHandlerType,
                                                  this,
                                                  typeof(AppKitLogoutListener)
                                                    .GetMethod(nameof(OnNativeLogout),
                                                               BindingFlags.Instance|BindingFlags.NonPublic));
            evt.AddEventHandler(inst, handler);
            Debug.Log("[AppKitLogout] Abonné à " + evt.Name);
        }
    }

    // Méthode appelée par l'event
    void OnNativeLogout()
    {
        Debug.Log("[AppKitLogout] Déconnexion native détectée");
        WalletManager.Instance?.Disconnect();
    }
}*/