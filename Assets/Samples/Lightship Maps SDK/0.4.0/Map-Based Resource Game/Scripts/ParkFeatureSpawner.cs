using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ParkFeatureSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject parkFeaturePrefab;

    [SerializeField]
    private int numberOfFeatures = 500;
    [SerializeField]
    private float spawnRadius = 150f;

    [SerializeField]
    private int extraFeaturesCount = 60; // Nombre de features spawnées lors d'une utilisation du bouton
    [SerializeField]
    private float extraSpawnRadius = 30f; // Rayon d'accès pour les extra spawns

    public static int extraSpawnRemaining = 60;

    [SerializeField]
    private TextMeshProUGUI extraSpawnCounterText;

    [Serializable]
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

    [Serializable]
    public class FeatureDataList
    {
        public List<FeatureData> features = new List<FeatureData>();
    }

    private static List<FeatureData> spawnedFeatures = new List<FeatureData>();

    private const string FeaturesKey = "ParkFeaturesState";
    private const string ResetDateKey = "FeaturesLastResetDate";
    private const string ExtraSpawnKey = "ExtraSpawnRemaining";

    void Start()
    {
        DailyResetCheck(); 
        LoadFeatures();
        UpdateExtraSpawnCounterUI();

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

    private void DailyResetCheck()
    {
        string todayUTC = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string lastReset = PlayerPrefs.GetString(ResetDateKey, "");

        if (todayUTC != lastReset)
        {
            spawnedFeatures.Clear();
            PlayerPrefs.DeleteKey(FeaturesKey);

            extraSpawnRemaining = 8;
            PlayerPrefs.SetInt(ExtraSpawnKey, extraSpawnRemaining);

            PlayerPrefs.SetString(ResetDateKey, todayUTC);
            PlayerPrefs.Save();
        }
        else
        {
            extraSpawnRemaining = PlayerPrefs.GetInt(ExtraSpawnKey, 8);
        }
    }

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

    public void SpawnFeaturesExtra()
    {
        if (extraSpawnRemaining <= 0)
        {
            Debug.Log("Aucun extra spawn restant pour aujourd'hui.");
            return;
        }

        extraSpawnRemaining--; 
        PlayerPrefs.SetInt(ExtraSpawnKey, extraSpawnRemaining);
        PlayerPrefs.Save();
        UpdateExtraSpawnCounterUI();

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

    private void UpdateExtraSpawnCounterUI()
    {
        if (extraSpawnCounterText != null)
            extraSpawnCounterText.text = "Spawn: " + extraSpawnRemaining;
    }

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
}