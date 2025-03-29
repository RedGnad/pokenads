using UnityEngine;

public enum MonsterType
{
    Mouch,
    Chog
}

public class MonsterSpawn : MonoBehaviour
{
    [SerializeField] private GameObject mouchModel; 
    [SerializeField] private GameObject chogModel;  
    [SerializeField] private float captureDelay = 5f; 

    public MonsterType selectedMonster;

    private void Start()
    {
        float rand = Random.value;
        if (rand < 0.90f)
        {
            mouchModel.SetActive(true);
            chogModel.SetActive(false);
            selectedMonster = MonsterType.Mouch;
        }
        else
        {
            mouchModel.SetActive(false);
            chogModel.SetActive(true);
            selectedMonster = MonsterType.Chog;
        }
    }

    public void TriggerCapture()
    {
        CaptureManager.CheckCapture(captureDelay, selectedMonster.ToString());
    }
}