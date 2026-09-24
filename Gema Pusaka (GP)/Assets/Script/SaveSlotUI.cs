using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("基礎交互")]
    public Button slotButton;

    [Header("狀態容器 (用於控制顯隱)")]
    public GameObject emptyStateObject;   // 空存檔時顯示的物體
    public GameObject usedStateObject;    // 已使用時顯示的物體

    [Header("已使用狀態下的 TMP 文本")]
    public TextMeshProUGUI zoneNameText;  // 改為顯示大區域名稱 (Zone)
    public TextMeshProUGUI progressText;  // 顯示遊戲進度

    [Header("技能解鎖圖標 (Icon)")]
    [Tooltip("順序對應：0-Serunai, 1-Gong, 2-Gendang, 3-Nobat")]
    public Image[] skillIcons = new Image[4]; 
    
    [Header("圖標顏色設置")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = new Color(1, 1, 1, 0.2f);

    private int mySlotIndex;
    private DataManager dataManager;

    // 由 DataManager 呼叫來初始化此槽位
    public void Initialize(int slotIndex, DataManager manager)
    {
        mySlotIndex = slotIndex;
        dataManager = manager;
        RefreshUI();
    }

    public void RefreshUI()
    {
        bool exists = PlayerPrefs.GetInt("Slot_" + mySlotIndex + "_Exists", 0) == 1;

        if (exists)
        {
            if (emptyStateObject != null) emptyStateObject.SetActive(false);
            if (usedStateObject != null) usedStateObject.SetActive(true);

            // 1. 讀取並顯示場景名 (作為 Zone Name)
            string sceneName = PlayerPrefs.GetString("Slot_" + mySlotIndex + "_Scene", "Unknown Zone");
            if (zoneNameText != null) zoneNameText.text = "Zone: " + sceneName;

            // 2. 讀取並顯示進度
            int progress = PlayerPrefs.GetInt("Slot_" + mySlotIndex + "_Progress", 0);
            if (progressText != null) progressText.text = "Progress: " + progress + "%";

            // 3. 讀取並更新 4 個技能的 Icon 狀態
            for (int s = 0; s < skillIcons.Length; s++)
            {
                if (skillIcons[s] != null)
                {
                    int isUnlocked = PlayerPrefs.GetInt("Slot_" + mySlotIndex + "_Skill_" + s, 0);
                    skillIcons[s].color = (isUnlocked == 1) ? unlockedColor : lockedColor;
                }
            }
        }
        else
        {
            if (emptyStateObject != null) emptyStateObject.SetActive(true);
            if (usedStateObject != null) usedStateObject.SetActive(false);
        }

        // 綁定按鈕點擊事件，通知 DataManager 處理
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => dataManager.OnSlotClicked(mySlotIndex, exists));
    }
}