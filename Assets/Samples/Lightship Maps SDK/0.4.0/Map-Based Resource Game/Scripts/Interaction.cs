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

    void Start()
    {
        myCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (retourButton != null)
            retourButton.SetActive(false);
    }

    void Update()
    {
        if (score >= 20)
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

                if (score >= 20)
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.AddScore(20);
                    if (retourButton != null)
                        retourButton.SetActive(true);

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

                    string monsterType = "unknown";
                    if (monsterSpawnReference != null)
                        monsterType = monsterSpawnReference.selectedMonster.ToString();

                    CaptureManager.CheckCapture(5f, monsterType);

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