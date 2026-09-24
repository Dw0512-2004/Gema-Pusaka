using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NusaPromptManager : MonoBehaviour
{
    public static NusaPromptManager Instance;

    [Header("UI 引用")]
    [Tooltip("Nusa 提示框的文字")]
    public TextMeshProUGUI promptText;
    
    [Tooltip("Nusa 提示框的小圖標 Image (可選)")]
    public Image promptIconImage;

    // --- 新增：狀態暫存 ---
    private string currentMessage = "";
    private Sprite currentIcon = null;
    private bool isSuppressed = false; // 是否被技能面板強制隱藏中

    private void Awake()
    {
        Instance = this;

        if (promptText == null)
            promptText = GetComponentInChildren<TextMeshProUGUI>(true);

        HidePrompt();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 支援傳入文字與選填的 Icon
    public void ShowPrompt(string message, Sprite icon = null)
    {
        // 1. 先把內容存起來，以防需要恢復
        currentMessage = message;
        currentIcon = icon;

        // 2. 如果目前正在被壓制（技能面板開著），只存數據，不顯示 UI
        if (isSuppressed) return;

        // 3. 正常顯示 UI
        if (promptText != null)
        {
            promptText.text = currentMessage;
            promptText.gameObject.SetActive(true);
        }

        if (promptIconImage != null)
        {
            if (currentIcon != null)
            {
                promptIconImage.sprite = currentIcon;
                promptIconImage.gameObject.SetActive(true);
            }
            else
            {
                promptIconImage.gameObject.SetActive(false);
            }
        }
    }

    // 徹底關閉並清除資料 (當玩家離開互動範圍時調用)
    public void HidePrompt()
    {
        currentMessage = "";
        currentIcon = null;

        if (promptText != null)
        {
            promptText.text = "";
            promptText.gameObject.SetActive(false);
        }

        if (promptIconImage != null)
        {
            promptIconImage.gameObject.SetActive(false);
        }
    }

    // ==========================================
    // 給 Skill Panel 調用的新方法
    // ==========================================
    
    // 面板開啟時呼叫：暫時隱藏 UI，但保留資料
    public void SuppressPrompt()
    {
        isSuppressed = true;
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (promptIconImage != null) promptIconImage.gameObject.SetActive(false);
    }

    // 面板關閉時呼叫：解除隱藏，如果有暫存的資料就重新顯示
    public void RestorePrompt()
    {
        isSuppressed = false;
        
        // 如果原本有文字，就重新呼叫 ShowPrompt 顯示出來
        if (!string.IsNullOrEmpty(currentMessage))
        {
            ShowPrompt(currentMessage, currentIcon);
        }
    }
}