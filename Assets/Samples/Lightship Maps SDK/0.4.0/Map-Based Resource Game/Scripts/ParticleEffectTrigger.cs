using UnityEngine;

public class ParticleEffectTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject particlePrefab;

    [SerializeField]
    private Transform effectSpawnPoint;

    public void TriggerEffect()
    {
        if (particlePrefab != null)
        {
            Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            GameObject effect = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Particle prefab non assigné dans ParticleEffectTrigger sur " + gameObject.name);
        }
    }
}