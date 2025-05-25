using UnityEngine;

/// <summary>
/// Classe statique globale pour suivre l'état du modal de connexion wallet
/// Accessible depuis tous les namespaces sans import spécifique
/// </summary>
public static class WalletModalState
{
    /// <summary>
    /// Indique si le modal de connexion wallet est actuellement ouvert
    /// </summary>
    public static bool IsModalOpen { get; set; } = false;
}