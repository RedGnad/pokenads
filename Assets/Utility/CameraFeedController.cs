// CameraFeedController.cs
using UnityEngine;

public class CameraFeedController : MonoBehaviour
{
    [Tooltip("Référence à tous vos modèles 3D d'armes, dans l'ordre des indexes")]
    public GameObject[] weaponPrefabs;

    void Start()
    {
        int idx = WeaponSelectionManager.SelectedWeaponIndex;

        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            weaponPrefabs[i].SetActive(i == idx);
        }
    }
}
