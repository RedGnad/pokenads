using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class Interaction : MonoBehaviour
{
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public GameObject retourButton;
    public ParticleSystem particleSystemPrefab;
    public GameObject vfxPrefab;          
    public GameObject extraVfxPrefab;       
    public GameObject thirdVfxPrefab;       
    public float thirdVfxDelay = 5f;

    public AudioClip interactionSound;  
    public AudioClip disappearanceSound;   
    public AudioClip secondDisappearanceSound; 

    private AudioSource audioSource;
    private Collider myCollider;

    public MonsterSpawn monsterSpawnReference;
    private int requiredClicks = 20; // Valeur par défaut
    private bool pvAdjusted = false;
    private float adjustmentTimer = 0f;
    private const float PV_CHECK_TIMEOUT = 2.0f; // Temps maximum pour ajuster les PV

    void Start()
    {
        myCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (retourButton != null)
            retourButton.SetActive(false);
            
        // Afficher le score sans le total
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score;
        }
        
        // Essayer d'ajuster les PV immédiatement
        AdjustRequiredClicks();
        
        // Et aussi lancer une coroutine pour une vérification différée
        StartCoroutine(DelayedPVCheck());
    }
    
    // Méthode principale pour ajuster les PV en fonction du monstre
    private bool AdjustRequiredClicks()
    {
        if (monsterSpawnReference == null)
        {
            monsterSpawnReference = GetComponent<MonsterSpawn>();
            if (monsterSpawnReference == null)
            {
                monsterSpawnReference = FindObjectOfType<MonsterSpawn>();
                if (monsterSpawnReference == null)
                {
                    Debug.LogWarning("[Interaction] Impossible de trouver un MonsterSpawn!");
                    return false;
                }
            }
        }
        
        // Déterminer les PV en fonction du type de monstre
        switch (monsterSpawnReference.selectedMonster)
        {
            case MonsterType.Moyaki:
                requiredClicks = 30;
                break;
            case MonsterType.Molandak: // NOUVEAU: Cas pour Molandak
                requiredClicks = 50;
                break;
            default: // Mouch, Chog et autres
                requiredClicks = 20;
                break;
        }
        
        Debug.Log($"[Interaction] PV ajustés pour {monsterSpawnReference.selectedMonster} : {requiredClicks}");
        return true;
    }
    
    private IEnumerator DelayedPVCheck()
    {
        // Attendre un peu plus longtemps pour s'assurer que MonsterSpawn a eu le temps d'initialiser
        yield return new WaitForSeconds(0.3f);
        
        bool success = AdjustRequiredClicks();
        
        if (success)
        {
            pvAdjusted = true;
            Debug.Log($"[Interaction] PV vérifiés avec succès après délai : {requiredClicks}");
        }
    }

    void Update()
    {
        // Si les PV n'ont pas encore été correctement ajustés, continuer à essayer
        if (!pvAdjusted)
        {
            adjustmentTimer += Time.deltaTime;
            if (adjustmentTimer > 0.5f)
            {
                adjustmentTimer = 0f;
                pvAdjusted = AdjustRequiredClicks();
                
                if (adjustmentTimer > PV_CHECK_TIMEOUT)
                {
                    // Abandonner après le temps imparti
                    pvAdjusted = true;
                    Debug.LogWarning("[Interaction] Échec d'ajustement des PV, utilisation de la valeur par défaut");
                }
            }
        }
        
        if (score >= requiredClicks)
        {
            if (retourButton != null && !retourButton.activeSelf)
                retourButton.SetActive(true);
        }
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            ProcessInput(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
    }

    void ProcessInput(Vector3 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                if (interactionSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(interactionSound);
                }

                score++;
                if (scoreText != null)
                {
                    scoreText.text = "Score : " + score;
                }

                if (score >= requiredClicks)
                {
                    if (GameManager.Instance != null)
                    {
                        string monsterType = "unknown";
                        int scorePoints = 20; // Valeur par défaut
                        
                        if (monsterSpawnReference != null)
                        {
                            monsterType = monsterSpawnReference.selectedMonster.ToString();
                            
                            // Attribution de points selon le type
                            if (monsterType == "Moyaki")
                            {
                                scorePoints = 30;
                            }
                            else if (monsterType == "Molandak") // NOUVEAU: Points pour Molandak
                            {
                                scorePoints = 50;
                            }
                            
                            PlayerPrefs.SetString("CapturedMonsterType", monsterType);
                            PlayerPrefs.SetInt("TempNadsScore", scorePoints);
                            PlayerPrefs.Save();
                            
                            Debug.Log($"[Interaction] Monstre {monsterType} vaincu, {scorePoints} points attribués");
                            
                            GameManager.Instance.AddScore(scorePoints);
                            
                            if (monsterSpawnReference != null && monsterSpawnReference.gameObject != null)
                            {
                                MonsterSpawn.ActiveMonster.TriggerCapture();
                                
                                // Masquer les modèles de monstres (ajout pour résoudre le problème)
                                HideMonsterModels();
                            }
                            
                            GameEvents.NotifyCombatFinished(monsterType);
                        }
                    }
                    
                    if (retourButton != null)
                        retourButton.SetActive(true);

                    if (vfxPrefab != null)
                    {
                        Instantiate(vfxPrefab, transform.position, Quaternion.identity);
                    }
                    if (extraVfxPrefab != null)
                    {
                        Instantiate(extraVfxPrefab, transform.position, Quaternion.identity);
                    }

                    if (disappearanceSound != null)
                    {
                        audioSource.PlayOneShot(disappearanceSound);
                    }

                    if (secondDisappearanceSound != null)
                    {
                        audioSource.PlayOneShot(secondDisappearanceSound);
                    }

                    StartCoroutine(SpawnThirdVfx());
                    
                    if (myCollider != null)
                        myCollider.enabled = false;
                }
                if (particleSystemPrefab != null)
                {
                    Instantiate(particleSystemPrefab, hit.point, Quaternion.identity);
                }
            }
        }
    }
    
    // Nouvelle méthode pour masquer tous les modèles de monstres
    private void HideMonsterModels()
    {
        // Désactiver ce GameObject pour le masquer
        if (gameObject != null)
        {
            // Désactiver les Renderers pour masquer le modèle visuellement
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
            
            // Pour Molandak spécifiquement
            GameObject molandakModel = GameObject.Find("MolandakModel");
            if (molandakModel != null)
            {
                molandakModel.SetActive(false);
            }
        }
        
        // En cas de structure différente, essayer de trouver et masquer les modèles par nom
        if (monsterSpawnReference != null)
        {
            foreach (Transform child in monsterSpawnReference.transform)
            {
                if (child.name.Contains("Model"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    IEnumerator SpawnThirdVfx()
    {
        yield return new WaitForSeconds(thirdVfxDelay);
        if (thirdVfxPrefab != null)
        {
            Instantiate(thirdVfxPrefab, transform.position, Quaternion.identity);
        }
    }

    public void RetourEcranPrincipal()
    {
        SceneTransitionData.LoadingTime = 1f;
        SceneManager.LoadScene("MapScreen");
    }
}