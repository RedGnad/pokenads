using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField]
    private GameObject loadingScreenPanel;

    [SerializeField]
    private float minimumLoadingTime = 3f;

    void Start()
    {
        if (loadingScreenPanel != null)
        {
            loadingScreenPanel.SetActive(true);
            StartCoroutine(HideLoadingScreen());
        }
    }

    private IEnumerator HideLoadingScreen()
    {
        yield return new WaitForSeconds(minimumLoadingTime);
        loadingScreenPanel.SetActive(false);
    }
}