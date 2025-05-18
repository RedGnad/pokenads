using UnityEngine;
using System;

/// <summary>
/// Système d'événements global pour la communication entre scripts sans dépendances directes
/// </summary>
public static class GameEvents
{
    // Événement déclenché quand un combat est terminé
    public static event Action<string> OnCombatFinished;

    // Méthode pour notifier que le combat est terminé
    public static void NotifyCombatFinished(string monsterType)
    {
        Debug.Log($"GameEvents: Combat terminé avec monstre de type {monsterType}");
        OnCombatFinished?.Invoke(monsterType);
    }
}