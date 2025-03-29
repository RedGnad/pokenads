using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Dictionary<string, int> capturedMonsters = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCapturedMonster(string monsterType)
    {
        if (capturedMonsters.ContainsKey(monsterType))
        {
            capturedMonsters[monsterType]++;
        }
        else
        {
            capturedMonsters[monsterType] = 1;
        }
        SaveInventory();
        Debug.Log("Monstre capturé : " + monsterType + ". Total : " + capturedMonsters[monsterType]);
    }

    public void SaveInventory()
    {
        foreach (KeyValuePair<string, int> pair in capturedMonsters)
        {
            PlayerPrefs.SetInt("CapturedMonster_" + pair.Key, pair.Value);
        }
        PlayerPrefs.Save();
        Debug.Log("Inventaire sauvegardé.");
    }

    public void LoadInventory()
    {
        string[] knownMonsters = new string[] { "Mouch", "Chog" };
        capturedMonsters.Clear();
        foreach (string type in knownMonsters)
        {
            int count = PlayerPrefs.GetInt("CapturedMonster_" + type, 0);
            if (count > 0)
            {
                capturedMonsters[type] = count;
            }
        }
        Debug.Log("Inventaire chargé.");
    }
}