// Un nouveau script, n'affecte pas vos scripts existants
using UnityEngine;

public class MapSoundController : MonoBehaviour
{
    public AudioSource mapSound;
    private static bool isFirstLoad = true;

    void Start()
    {
        // Ne jouez pas le son au premier lancement
        if (isFirstLoad)
        {
            isFirstLoad = false;
            Debug.Log("Premier lancement, son non joué");
        }
        else
        {
            // Jouer le son seulement lors d'un retour
            if (mapSound != null)
            {
                mapSound.Play();
                Debug.Log("Son joué - retour à la map");
            }
        }
    }
}