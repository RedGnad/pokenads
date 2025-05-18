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
    private static MonsterSpawn activeMonster;
    
    [SerializeField] private float respawnDelay = 30000f;
    [SerializeField] private GameObject mouchModel; 
    [SerializeField] private GameObject chogModel;  
    [SerializeField] private GameObject moyakiModel;
    [SerializeField] private float captureDelay = 5f;

    [Header("Probabilités d'apparition")]
    [Range(0, 100)]
    [SerializeField] private float mouchChance = 70f;
    [Range(0, 100)]
    [SerializeField] private float chogChance = 20f;
    [Range(0, 100)]
    [SerializeField] private float moyakiChance = 10f;

    // Cette variable est une copie LOCALE du type de monstre
    [SerializeField] private MonsterType _selectedMonster;
    
    // Propriété pour lire le type en externe
    public MonsterType selectedMonster 
    { 
        get { return _selectedMonster; } 
        private set { _selectedMonster = value; }
    }
    
    public static MonsterSpawn ActiveMonster { get { return activeMonster; } }

    private void Awake()
    {
        // Nettoyer les références entre les sessions
        if (activeMonster != null && activeMonster != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        
        activeMonster = this;
        
        // IMPORTANT: Forcer une nouvelle sélection
        _selectedMonster = MonsterType.Mouch; // Valeur par défaut
    }

    private void Start()
    {
        // CRUCIAL: Forcer une nouvelle sélection à chaque lancement
        // pour éviter les problèmes de persistance entre scènes
        SelectRandomMonster();
        Debug.Log($"[MonsterSpawn] Start - Monstre sélectionné: {_selectedMonster}");
    }
    
    private void SelectRandomMonster()
    {
        // Désactiver tous les modèles
        if (mouchModel != null) mouchModel.SetActive(false);
        if (chogModel != null) chogModel.SetActive(false);
        if (moyakiModel != null) moyakiModel.SetActive(false);
        
        // Déterminer le type aléatoirement
        float totalChance = mouchChance + chogChance + moyakiChance;
        float normalizedMouchChance = mouchChance / totalChance;
        float normalizedChogChance = chogChance / totalChance;
        
        float rand = Random.value;
        
        // Assigner le type selon la probabilité
        if (rand < normalizedMouchChance) 
        {
            _selectedMonster = MonsterType.Mouch;
        }
        else if (rand < normalizedMouchChance + normalizedChogChance) 
        {
            _selectedMonster = MonsterType.Chog;
        }
        else 
        {
            _selectedMonster = MonsterType.Moyaki;
        }
        
        // IMPORTANT: Activer le modèle correspondant au type choisi
        switch (_selectedMonster)
        {
            case MonsterType.Mouch:
                if (mouchModel != null) mouchModel.SetActive(true);
                break;
            case MonsterType.Chog:
                if (chogModel != null) chogModel.SetActive(true);
                break;
            case MonsterType.Moyaki:
                if (moyakiModel != null) moyakiModel.SetActive(true);
                break;
        }
        
        // Sauvegarder pour les autres scripts
        PlayerPrefs.SetString("CurrentMonsterType", _selectedMonster.ToString());
        PlayerPrefs.SetInt("RequiredClicks", GetRequiredClicks());
        PlayerPrefs.Save();
        
        Debug.Log($"[MonsterSpawn] Type sélectionné: {_selectedMonster}, PV: {GetRequiredClicks()}");
    }

    public void TriggerCapture()
    {
        Debug.Log("Capture de " + _selectedMonster.ToString() + " en cours");
        CaptureManager.CheckCapture(captureDelay, _selectedMonster.ToString());
        
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
        
        GameObject newMonster = Instantiate(gameObject, transform.position, Quaternion.identity);
        MonsterSpawn newMonsterScript = newMonster.GetComponent<MonsterSpawn>();
        if (newMonsterScript != null)
        {
            // Forcer une nouvelle sélection
            newMonsterScript.SelectRandomMonster();
        }
        
        Destroy(gameObject);
    }
    
    public static void EnsureMonsterExists(GameObject monsterPrefab, Vector3 position)
    {
        if (activeMonster == null && monsterPrefab != null)
        {
            Instantiate(monsterPrefab, position, Quaternion.identity);
        }
    }
    
    // Méthode simplifiée ne dépendant QUE de _selectedMonster
    public int GetMonsterPoints()
    {
        // Moyaki vaut 30 points, les autres types 20
        return _selectedMonster == MonsterType.Moyaki ? 30 : 20;
    }
    
    // Méthode simplifiée ne dépendant QUE de _selectedMonster
    public int GetRequiredClicks()
    {
        // Moyaki nécessite 30 clics, les autres types 20
        return _selectedMonster == MonsterType.Moyaki ? 30 : 20;
    }
}