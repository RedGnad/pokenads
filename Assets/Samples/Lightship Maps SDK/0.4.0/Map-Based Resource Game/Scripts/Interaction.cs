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
    public GameObject vfxPrefab;           // Premier VFX
    public GameObject extraVfxPrefab;      // Deuxième VFX
    public GameObject thirdVfxPrefab;      // Troisième VFX
    public float thirdVfxDelay = 5f;

    public AudioClip interactionSound;      // Son lancé à chaque interaction
    public AudioClip disappearanceSound;      // Premier son de disparition
    public AudioClip secondDisappearanceSound; // Deuxième son de disparition

    private AudioSource audioSource;
    private Collider myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Ajoute un AudioSource si l'objet n'en possède pas
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (retourButton != null)
        {
            retourButton.SetActive(false);
        }
    }

    void Update()
    {
        // Une fois le score atteint, ne plus gérer d'interaction
        if (score >= 20)
            return;

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
                // Son d'interaction
                if (interactionSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(interactionSound);
                }

                score++;
                if (scoreText != null)
                {
                    scoreText.text = "Score : " + score;
                }

                if (score >= 20)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddScore(20);
                    }
                    if (retourButton != null)
                    {
                        retourButton.SetActive(true);
                    }
                    // Instanciation du premier VFX
                    if (vfxPrefab != null)
                    {
                        GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
                        Destroy(vfx, 5f);
                    }
                    // Instanciation du deuxième VFX
                    if (extraVfxPrefab != null)
                    {
                        GameObject extraVfx = Instantiate(extraVfxPrefab, transform.position, Quaternion.identity);
                        Destroy(extraVfx, 5f);
                    }
                    // Lancement d'une coroutine pour déclencher le troisième VFX après délai
                    StartCoroutine(SpawnThirdVfx());

                    // Jouer les sons de disparition
                    if (disappearanceSound != null)
                    {
                        AudioSource.PlayClipAtPoint(disappearanceSound, transform.position);
                    }
                    if (secondDisappearanceSound != null)
                    {
                        AudioSource.PlayClipAtPoint(secondDisappearanceSound, transform.position);
                    }
                    
                    // Délai de 5 secondes et vérification de capture via le CaptureManager
                    CaptureManager.CheckCapture(5f);
                    
                    // Désactivation de l'objet (les VFX et la capture continueront via CaptureManager)
                    gameObject.SetActive(false);
                }

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