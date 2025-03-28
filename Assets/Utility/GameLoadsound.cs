using UnityEngine;

public class SceneLoadSound : MonoBehaviour
{
    public AudioClip loadSound;  
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (loadSound != null)
        {
            audioSource.PlayOneShot(loadSound);
        }
    }
}