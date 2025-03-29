using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleTextController : MonoBehaviour
{
    public TMP_Text uiText;  
    public float displayDuration = 2f;  

    public void ShowMessage(string message)
    {
        if(uiText != null)
        {
            uiText.text = message;
            StartCoroutine(ClearTextAfterDelay());
        }
        else
        {
            Debug.LogWarning("SimpleTextController : Aucun composant TMP_Text n'est assigné.");
        }
    }

    private IEnumerator ClearTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        uiText.text = "";  
    }
}
