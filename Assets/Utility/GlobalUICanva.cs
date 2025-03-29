using UnityEngine;
using UnityEngine.UI;

public class GlobalUICanvas : MonoBehaviour
{
    public static GlobalUICanvas Instance;
    
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
}