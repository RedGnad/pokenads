using UnityEngine;
using TMPro; 

public class SimpleTextDisplay : MonoBehaviour
{
    public TMP_Text uiText; 

    void Start()
    {
        if (uiText != null)
        {
            uiText.text = "Capture réussie !";
        }
        else
        {
            Debug.LogWarning("Le composant TMP_Text n'est pas assigné.");
        }
    }
}
