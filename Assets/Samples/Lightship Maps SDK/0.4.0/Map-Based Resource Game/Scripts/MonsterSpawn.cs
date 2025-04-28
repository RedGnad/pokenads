using UnityEngine;
using System.Collections;

public enum MonsterType
{
    Mouch,
    Chog,
    Moyaki
}

public class MonsterSpawn : MonoBehaviour
{
    // Référence statique au monstre actif
    private static MonsterSpawn activeMonster;
    
    // Temps avant l'apparition d'un nouveau monstre (en secondes)
    [SerializeField] private float respawnDelay = 30000f;
    
    [SerializeField] private GameObject mouchModel; 
    [SerializeField] private GameObject chogModel;  
    [SerializeField] private GameObject moyakiModel;
    [SerializeField] private float captureDelay = 5f;

    public MonsterType selectedMonster;

    private void Awake()
    {
        // Si un monstre est déjà actif, désactiver celui-ci
        if (activeMonster != null && activeMonster != this)
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Définir ce monstre comme actif
        activeMonster = this;
    }

    private void Start()
    {
        SelectRandomMonster();
    }
    
    private void SelectRandomMonster()
    {
        // Désactiver tous les modèles d'abord
        if (mouchModel != null) mouchModel.SetActive(false);
        if (chogModel != null) chogModel.SetActive(false);
        if (moyakiModel != null) moyakiModel.SetActive(false);
        
        float rand = Random.value;
        
        if (rand < 0.70f) // 70% de chance pour Mouch
        {
            if (mouchModel != null) mouchModel.SetActive(true);
            selectedMonster = MonsterType.Mouch;
        }
        else if (rand < 0.90f) // 20% de chance pour Chog (0.70 à 0.90)
        {
            if (chogModel != null) chogModel.SetActive(true);
            selectedMonster = MonsterType.Chog;
        }
        else // 10% de chance pour Moyaki (0.90 à 1.00)
        {
            if (moyakiModel != null) moyakiModel.SetActive(true);
            selectedMonster = MonsterType.Moyaki;
        }
        
        Debug.Log("Monstre sélectionné: " + selectedMonster.ToString());
    }

    public void TriggerCapture()
    {
        Debug.Log("Capture de " + selectedMonster.ToString() + " en cours");
        CaptureManager.CheckCapture(captureDelay, selectedMonster.ToString());
        
        // Supprimer ce monstre et planifier l'apparition d'un nouveau
        if (this == activeMonster)
        {
            activeMonster = null;
            StartCoroutine(RespawnAfterDelay());
            gameObject.SetActive(false);
        }
    }
    
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        // Créer un nouveau monstre
        GameObject newMonster = Instantiate(gameObject, transform.position, Quaternion.identity);
        newMonster.SetActive(true);
        
        // Supprimer ce gameObject
        Destroy(gameObject);
    }
    
    // Méthode statique pour créer un monstre s'il n'y en a pas déjà un
    public static void EnsureMonsterExists(GameObject monsterPrefab, Vector3 position)
    {
        if (activeMonster == null && monsterPrefab != null)
        {
            Instantiate(monsterPrefab, position, Quaternion.identity);
        }
    }
}