using UnityEngine;
using System.Collections;

public class CaptureManager : MonoBehaviour
{
    public static int capturedCount = 0;

    public static void CheckCapture(float delay)
    {
        // Crée un objet temporaire pour exécuter la coroutine
        GameObject temp = new GameObject("CaptureManagerTemp");
        CaptureManager cm = temp.AddComponent<CaptureManager>();
        cm.StartCoroutine(cm.CaptureCoroutine(delay));
    }

    private IEnumerator CaptureCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 10% de chance de capture
        if (Random.value < 0.1f)
        {
            capturedCount++;
            Debug.Log("Capture réussi ! Nombre total de modèles capturés : " + capturedCount);
        }
        else
        {
            Debug.Log("Capture ratée.");
        }
        Destroy(gameObject);
    }
}