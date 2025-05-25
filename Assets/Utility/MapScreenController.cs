// MapScreenController.cs
using UnityEngine;

public class MapScreenController : MonoBehaviour
{
    /// <summary>
    /// Appelé quand on clique sur un bouton d'arme.
    /// Ne charge plus la scène, juste mémorise l'index.
    /// </summary>
    public void OnWeaponButtonClicked(int weaponIndex)
    {
        WeaponSelectionManager.SelectedWeaponIndex = weaponIndex;
        Debug.Log($"Arme sélectionnée : {weaponIndex}");
        // plus d'appel à LoadScene ici
    }
}
