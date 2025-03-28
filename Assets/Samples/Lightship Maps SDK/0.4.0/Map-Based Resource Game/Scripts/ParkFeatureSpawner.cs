using System.Collections.Generic;
using UnityEngine;
using System;

public class ParkFeatureSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject parkFeaturePrefab;
    // Paramètres d'initialisation habituels
    [SerializeField]
    private int numberOfFeatures = 10;
    [SerializeField]
    private float spawnRadius = 50f;

    [System.Serializable]
    public class FeatureData
    {
        public float posX;
        public float posY;
        public float posZ;
        public bool collected;

        public FeatureData(Vector3 position, bool collected)
        {
            posX = position.x;
            posY = position.y;
            posZ = position.z;
            this.collected = collected;
        }
    
        public Vector3 GetPosition()
        {
            return new Vector3(posX, posY, posZ);
        }
    }

    [System.Serializable]
    public class FeatureDataList
    {
        public List<FeatureData> features = new List<FeatureData>();
    }

    // Liste persistante de features
    private static List<FeatureData> spawnedFeatures = new List<FeatureData>();
    private const string FeaturesKey = "ParkFeaturesState";
    private const string ResetDateKey = "FeaturesLastResetDate";

    void Start()
    {
        DailyResetCheck(); // Réinitialise à minuit UTC si nécessaire
        LoadFeatures();
    
        if (spawnedFeatures.Count > 0)
        {
            foreach (FeatureData feature in spawnedFeatures)
            {
                if (!feature.collected)
                    Instantiate(parkFeaturePrefab, feature.GetPosition(), Quaternion.identity);
            }
        }
        else
        {
            SpawnFeaturesAroundPlayer();
        }
    }

    /// <summary>
    /// Vérifie s'il s'agit d'un nouveau jour UTC. Si oui, réinitialise les features.
    /// </summary>
    private void DailyResetCheck()
    {
        string todayUTC = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string lastReset = PlayerPrefs.GetString(ResetDateKey, "");

        if (todayUTC != lastReset)
        {
            spawnedFeatures.Clear();
            PlayerPrefs.DeleteKey(FeaturesKey);
            PlayerPrefs.SetString(ResetDateKey, todayUTC);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Génère les features initiales autour du joueur (avec les paramètres usuels).
    /// </summary>
    private void SpawnFeaturesAroundPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        Vector3 playerPos = player.transform.position;
        for (int i = 0; i < numberOfFeatures; i++)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(playerPos.x + randomPoint.x, playerPos.y, playerPos.z + randomPoint.y);
            FeatureData data = new FeatureData(spawnPos, false);
            spawnedFeatures.Add(data);
            Instantiate(parkFeaturePrefab, spawnPos, Quaternion.identity);
        }
        SaveFeatures();
    }

    /// <summary>
    /// Méthode publique pour générer 7 nouvelles features dans un rayon de 30 unités (rayon d'accessibilité) autour du joueur.
    /// Accessible via un bouton dans l'UI.
    /// </summary>
    public void SpawnFeaturesExtra()
    {
        int extraFeaturesCount = 7;
        float extraSpawnRadius = 30f; // Rayon d'accessibilité

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        Vector3 playerPos = player.transform.position;
        for (int i = 0; i < extraFeaturesCount; i++)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * extraSpawnRadius;
            Vector3 spawnPos = new Vector3(playerPos.x + randomPoint.x, playerPos.y, playerPos.z + randomPoint.y);
            FeatureData data = new FeatureData(spawnPos, false);
            spawnedFeatures.Add(data);
            Instantiate(parkFeaturePrefab, spawnPos, Quaternion.identity);
        }
        SaveFeatures();
    }

    /// <summary>
    /// Méthode publique pour marquer une feature comme collectée.
    /// </summary>
    public static void MarkFeatureCollected(Vector3 featurePosition)
    {
        float tolerance = 1.0f;
        foreach (FeatureData feature in spawnedFeatures)
        {
            if (!feature.collected && Vector3.Distance(feature.GetPosition(), featurePosition) < tolerance)
            {
                feature.collected = true;
                break;
            }
        }
        SaveFeaturesStatic();
    }

    private void SaveFeatures()
    {
        FeatureDataList wrapper = new FeatureDataList();
        wrapper.features = spawnedFeatures;
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(FeaturesKey, json);
        PlayerPrefs.Save();
    }

    private static void SaveFeaturesStatic()
    {
        FeatureDataList wrapper = new FeatureDataList();
        wrapper.features = spawnedFeatures;
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(FeaturesKey, json);
        PlayerPrefs.Save();
    }

    private void LoadFeatures()
    {
        if (PlayerPrefs.HasKey(FeaturesKey))
        {
            string json = PlayerPrefs.GetString(FeaturesKey);
            FeatureDataList wrapper = JsonUtility.FromJson<FeatureDataList>(json);
            if (wrapper != null && wrapper.features != null)
            {
                spawnedFeatures = wrapper.features;
            }
        }
    }
    
    // Les autres méthodes (ex: SpawnAdditionalFeatures) peuvent être ajoutées si nécessaire.
}