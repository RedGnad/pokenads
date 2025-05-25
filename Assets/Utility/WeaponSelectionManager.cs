// WeaponSelectionManager.cs
using UnityEngine;

public class WeaponSelectionManager : MonoBehaviour
{
    // Index de l'arme sélectionnée (0,1,2…)
    public static int SelectedWeaponIndex = 0;

    // (Optionnel) N'existe qu'une fois dans toute l’application
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
