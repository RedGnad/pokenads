using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class ScoreUpdater : MonoBehaviour
{
    [SerializeField] private string scoreTextObjectName = "ScoreText";
    private TextMeshProUGUI scoreText;
    private string lastText = "";
    private int tempScore = 0;
    private bool hasTempScore = false;
    
    void Start()
    {
        // S'abonner à l'événement de chargement de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Démarrer la surveillance des PlayerPrefs
        StartCoroutine(MonitorScoreUpdates());
        
        // Vérifier immédiatement s'il y a un score temporaire
        CheckTempScore();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Vérifier immédiatement s'il y a un score temporaire
        CheckTempScore();
        
        // Commencer à surveiller le texte d'affichage du score
        StartCoroutine(WatchScoreText());
    }
    
    private void CheckTempScore()
    {
        hasTempScore = PlayerPrefs.HasKey("TempNadsScore");
        if (hasTempScore)
        {
            tempScore = PlayerPrefs.GetInt("TempNadsScore");
            Debug.Log("Score temporaire détecté: " + tempScore);
        }
    }
    
    // Surveille activement le texte de score et le remplace si nécessaire
    private IEnumerator WatchScoreText()
    {
        yield return new WaitForSeconds(0.05f); // Court délai pour laisser l'UI s'initialiser
        
        // Boucle de surveillance pendant 5 secondes max (suffisant pour la mise à jour Firebase)
        for (int i = 0; i < 50; i++)
        {
            if (!hasTempScore) break; // Arrêter si pas de score temp
            
            // Trouver l'objet texte du score
            GameObject scoreObj = GameObject.Find(scoreTextObjectName);
            if (scoreObj != null)
            {
                scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
                if (scoreText != null)
                {
                    // Si le texte a changé et n'affiche pas notre score temporaire
                    if (scoreText.text != lastText && !scoreText.text.Contains(tempScore.ToString()))
                    {
                        scoreText.text = "Nads: " + tempScore;
                        Debug.Log("Remplacé le texte du score: " + scoreText.text + " par Nads: " + tempScore);
                    }
                    
                    lastText = scoreText.text;
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Surveiller et mettre à jour aussi dans Update pour être plus agressif
    void Update()
    {
        if (hasTempScore && scoreText != null && !FirebaseTest.Instance.IsScoreUpdateInProgress)
        {
            if (scoreText.text != "Nads: " + tempScore)
            {
                scoreText.text = "Nads: " + tempScore;
            }
        }
    }
    
    IEnumerator MonitorScoreUpdates()
    {
        while (true)
        {
            // Vérifier si une mise à jour Firebase est terminée
            if (!FirebaseTest.Instance.IsScoreUpdateInProgress && PlayerPrefs.HasKey("TempNadsScore"))
            {
                // Nettoyer les PlayerPrefs
                PlayerPrefs.DeleteKey("TempNadsScore");
                PlayerPrefs.DeleteKey("LastScoreTimestamp");
                PlayerPrefs.Save();
                
                // Indiquer qu'il n'y a plus de score temporaire
                hasTempScore = false;
                
                Debug.Log("ScoreUpdater: PlayerPrefs nettoyés après mise à jour Firebase");
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
}