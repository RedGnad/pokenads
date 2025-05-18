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
        
        // Vérifier si le monstre est un Moyaki
        bool isMoyaki = monsterSpawnReference.selectedMonster == MonsterType.Moyaki;
        
        // Ajuster les PV en conséquence
        int newRequiredClicks = isMoyaki ? 30 : 20;
        
        // Si les PV ont changé, mettre à jour et logger
        if (requiredClicks != newRequiredClicks)
        {
            requiredClicks = newRequiredClicks;
            Debug.Log($"[Interaction] PV ajustés pour {monsterSpawnReference.selectedMonster} : {requiredClicks}");
            return true;
        }
        
        return requiredClicks == 30 && isMoyaki; // Vrai si déjà correctement ajusté pour Moyaki
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
            
            // Essayer d'ajuster toutes les 0.5 secondes
            if (adjustmentTimer > 0.5f)
            {
                adjustmentTimer = 0f;
                pvAdjusted = AdjustRequiredClicks();
                
                // Abandonner après le délai maximal
                if (adjustmentTimer > PV_CHECK_TIMEOUT)
                {
                    Debug.LogWarning($"[Interaction] Délai d'ajustement PV dépassé, utilisation de {requiredClicks}");
                    pvAdjusted = true;
                }
            }
        }
        
        if (score >= requiredClicks)
            return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            ProcessInput(Input.GetTouch(0).position);
        else if (Input.GetMouseButtonDown(0))
            ProcessInput(Input.mousePosition);
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
                    audioSource.PlayOneShot(interactionSound);

                score++;
                if (scoreText != null)
                    scoreText.text = "Score : " + score;

                if (score >= requiredClicks)
                {
                    if (GameManager.Instance != null)
                    {
                        // Déterminer les points à ajouter en fonction des PV requis
                        int pointsToAdd = requiredClicks;
                        
                        // Double vérification pour Moyaki
                        string monsterType = "Mouch"; // Type par défaut
                        
                        if (monsterSpawnReference != null)
                        {
                            monsterType = monsterSpawnReference.selectedMonster.ToString();
                            
                            // Si c'est un Moyaki, forcer 30 points
                            if (monsterType == "Moyaki")
                            {
                                pointsToAdd = 30;
                            }
                        }
                        
                        // Ajout des points au score
                        GameManager.Instance.AddScore(pointsToAdd);
                        PlayerPrefs.SetInt("TempNadsScore", pointsToAdd);
                        PlayerPrefs.SetString("CapturedMonsterType", monsterType);
                        PlayerPrefs.Save();
                        
                        Debug.Log($"[Interaction] Monstre capturé: {monsterType}, Points: {pointsToAdd}");
                    }
                    
                    if (retourButton != null)
                        retourButton.SetActive(true);

                    // Effets visuels et sonores...
                    if (vfxPrefab != null)
                    {
                        GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
                        Destroy(vfx, 5f);
                    }
                    if (extraVfxPrefab != null)
                    {
                        GameObject extraVfx = Instantiate(extraVfxPrefab, transform.position, Quaternion.identity);
                        Destroy(extraVfx, 5f);
                    }
                    StartCoroutine(SpawnThirdVfx());

                    if (disappearanceSound != null)
                        AudioSource.PlayClipAtPoint(disappearanceSound, transform.position);
                    if (secondDisappearanceSound != null)
                        AudioSource.PlayClipAtPoint(secondDisappearanceSound, transform.position);

                    // Déterminer le type pour CaptureManager
                    string capturedType = (requiredClicks == 30) ? "Moyaki" : 
                                        (monsterSpawnReference != null ? 
                                         monsterSpawnReference.selectedMonster.ToString() : "Mouch");

                    // Lancer le processus de capture
                    CaptureManager.CheckCapture(5f, capturedType);
                    
                    // Désactiver l'objet
                    gameObject.SetActive(false);
                }

                // Effet de particules au clic
                if (particleSystemPrefab != null)
                {
                    ParticleSystem ps = Instantiate(particleSystemPrefab, hit.point, Quaternion.identity);
                    ps.Play();
                    Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
        }
    }

    IEnumerator SpawnThirdVfx()
    {
        yield return new WaitForSeconds(thirdVfxDelay);
        if (thirdVfxPrefab != null)
        {
            GameObject thirdVfx = Instantiate(thirdVfxPrefab, transform.position, Quaternion.identity);
            Destroy(thirdVfx, 5f);
        }
    }

    public void RetourEcranPrincipal()
    {
        SceneManager.LoadScene(0);
    }
}