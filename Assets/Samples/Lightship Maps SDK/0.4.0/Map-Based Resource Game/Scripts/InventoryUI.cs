using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    
    public TextMeshProUGUI mouchCountText;
    public TextMeshProUGUI chogCountText;

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool state = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(state);
            if (state)
                UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (GameManager.Instance != null)
        {
            int mouchCount = 0;
            int chogCount = 0;
            int count;
            if (GameManager.Instance.capturedMonsters.TryGetValue("Mouch", out count))
                mouchCount = count;
            if (GameManager.Instance.capturedMonsters.TryGetValue("Chog", out count))
                chogCount = count;

            if (mouchCountText != null)
                mouchCountText.text = "Mouch : " + mouchCount;
            if (chogCountText != null)
                chogCountText.text = "Skibidi Chog : " + chogCount;
        }
    }
}