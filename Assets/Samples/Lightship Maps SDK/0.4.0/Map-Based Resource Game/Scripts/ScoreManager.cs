using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button returnButton;

    private int score = 0;

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

        UpdateScoreUI();

        if (returnButton != null)
            returnButton.gameObject.SetActive(false);
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI();

        if (score >= 10 && returnButton != null)
            returnButton.gameObject.SetActive(true);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("GameSample");
    }
}
