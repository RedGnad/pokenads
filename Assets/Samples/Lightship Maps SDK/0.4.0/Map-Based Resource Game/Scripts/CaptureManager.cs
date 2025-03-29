using UnityEngine;
using System.Collections;

public class CaptureManager : MonoBehaviour
{
    public static CaptureManager Instance;
    
    public GameObject floatingTextPrefab;
    
    public AudioClip successSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void CheckCapture(float delay, string monsterType)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.CaptureRoutine(delay, monsterType));
        }
    }

    private IEnumerator CaptureRoutine(float delay, string monsterType)
    {
        yield return new WaitForSeconds(delay);

        bool captureResult = (Random.value < 0.20f);

        if (captureResult)
        {
            if (successSound != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(successSound, Camera.main.transform.position);
            }
            else
            {
                Debug.LogWarning("Effet sonore de capture réussie non assigné ou aucune caméra principale trouvée.");
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCapturedMonster(monsterType);
                Debug.Log("Monstre ajouté à l'inventaire: " + monsterType);
            }
            else
            {
                Debug.LogError("GameManager.Instance est nul ! Impossible d'ajouter le monstre à l'inventaire.");
            }
            
            if (floatingTextPrefab != null)
            {
                GameObject canvas = GameObject.FindWithTag("UICanvas");
                if (canvas != null)
                {
                    GameObject floatingTextInstance = Instantiate(floatingTextPrefab, canvas.transform);
                    
                    floatingTextInstance.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                    
                    FloatingText ftScript = floatingTextInstance.GetComponent<FloatingText>();
                    if (ftScript != null)
                    {
                        ftScript.SetText("Capture réussie !");
                    }
                    
                    Destroy(floatingTextInstance, 2f);
                }
                else
                {
                    Debug.LogWarning("Aucun Canvas avec le tag 'UICanvas' n'a été trouvé !");
                }
            }
        }
        else
        {
            Debug.Log("Capture échouée pour le monstre '" + monsterType + "'.");
        }

        Debug.Log("Capture finalisée pour le monstre '" + monsterType + "'. Succès : " + captureResult);
    }
}