using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public TMP_Text messageText;         
    public float displayDuration = 1.5f;   
    public float fadeDuration = 1f;        

    public void SetText(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            StartCoroutine(FadeOut());
        }
    }
    
    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        Color initialColor = messageText.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            messageText.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
