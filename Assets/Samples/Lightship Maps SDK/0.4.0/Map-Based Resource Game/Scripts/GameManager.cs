using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class MonsterCountItem
{
    public string monsterType;
    public int count;
}

[Serializable]
public class MonsterDictionaryWrapper
{
    public List<MonsterCountItem> items;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public int generalScore;
    public bool captureInProgress = false;
    public bool captureCompleted = false;
    public bool captureSuccess = false;
    
    public Dictionary<string, int> capturedMonsters = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCapturedMonsters();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveCapturedMonsters()
    {
        MonsterDictionaryWrapper wrapper = new MonsterDictionaryWrapper();
        wrapper.items = capturedMonsters.Select(kvp => new MonsterCountItem {
            monsterType = kvp.Key,
            count = kvp.Value
        }).ToList();
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("CapturedMonsters", json);
        PlayerPrefs.Save();
        Debug.Log("Données sauvegardées : " + json);
    }
    
    public void LoadCapturedMonsters()
    {
        if(PlayerPrefs.HasKey("CapturedMonsters"))
        {
            string json = PlayerPrefs.GetString("CapturedMonsters");
            MonsterDictionaryWrapper wrapper = JsonUtility.FromJson<MonsterDictionaryWrapper>(json);
            capturedMonsters.Clear(); 
            if (wrapper != null && wrapper.items != null)
            {
                foreach(var item in wrapper.items)
                {
                    capturedMonsters[item.monsterType] = item.count;
                }
            }
            Debug.Log("Données chargées : " + json);
        }
        else
        {
            Debug.Log("Aucune donnée sauvegardée trouvée.");
        }
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveCapturedMonsters();
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveCapturedMonsters();
    }

    public void AddScore(int score)
    {
        generalScore += score;
    }
    
    public void AddCapturedMonster(string monsterType)
    {
        if (capturedMonsters.ContainsKey(monsterType))
            capturedMonsters[monsterType]++;
        else
            capturedMonsters[monsterType] = 1;
        
        Debug.Log("Monstre '" + monsterType + "' capturé. Total : " + capturedMonsters[monsterType]);
        
        SaveCapturedMonsters();
    }
}