using UnityEngine;
using TMPro;

public class TutorialGuide : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("包含引導文字的整體 Panel (可選)")]
    public GameObject guidePanel;
    
    [Tooltip("引導內容的 TextMeshPro")]
    public TextMeshProUGUI guideText;

    [Header("設定")]
    [Tooltip("設定一個專屬ID (例如: MovementGuide)，避免跟其他教學衝突")]
    public string tutorialID = "FirstLevelGuide";
    
    [Tooltip("引導提示在畫面上停留的秒數")]
    public float displayDuration = 5f; // 🌟 新增：控制顯示秒數

    private string saveKey;
    private bool isCompleted = false;

    private void Start()
    {
        // 結合當前存檔槽位，確保不同存檔的教學進度是獨立的
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        saveKey = "Slot_" + activeSlot + "_Tutorial_" + tutorialID + "_Done";

        // 檢查這個存檔是否已經完成過這個教學
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            // 如果已經顯示過，直接在場景中將自己徹底銷毀，完全不佔效能
            Destroy(gameObject);
        }
        else
        {
            // 第一次進入，顯示引導
            if (guidePanel != null) guidePanel.SetActive(true);
            if (guideText != null) guideText.gameObject.SetActive(true);

            // 🌟 核心：設定 5 秒後自動呼叫 CompleteAndHideGuide 方法
            Invoke(nameof(CompleteAndHideGuide), displayDuration);
        }
    }

    private void OnDestroy()
    {
        // 如果玩家在 5 秒內就提早離開了場景，也要確保記錄被寫入
        if (!isCompleted && PlayerPrefs.GetInt(saveKey, 0) == 0)
        {
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
        }
    }

    // 時間到（或手動觸發）時執行：關閉 UI、寫入存檔並銷毀自己
    public void CompleteAndHideGuide()
    {
        isCompleted = true;
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();
        
        if (guidePanel != null) guidePanel.SetActive(false);
        if (guideText != null) guideText.gameObject.SetActive(false);
        
        // 隱藏後直接銷毀腳本與物件
        Destroy(gameObject);
    }
}