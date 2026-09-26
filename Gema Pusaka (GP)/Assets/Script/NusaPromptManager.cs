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

    // --- 狀態暫存 ---
    private string currentMessage = "";
    private Sprite currentIcon = null;
    private bool isSuppressed = false; // 是否被技能面板強制隱藏中
    private bool isNearPuzzle = false; // 是否碰到 Puzzle 標籤

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

    private void Update()
    {
        // 每一幀動態檢查是否需要隱藏提示框，確保不會與任何 Panel 發生重疊
        UpdateVisibility();
    }

    // 🌟 核心可視度控制邏輯
    private void UpdateVisibility()
    {
        bool shouldHide = isNearPuzzle || isSuppressed;

        // 檢查 NusaController 裡面的各大面板是否開啟
        if (NusaController.Instance != null)
        {
            if (NusaController.Instance.pauseMenuPanel != null && NusaController.Instance.pauseMenuPanel.activeSelf) shouldHide = true;
            if (NusaController.Instance.settingsPanel != null && NusaController.Instance.settingsPanel.activeSelf) shouldHide = true;
            if (NusaController.Instance.mapPanel != null && NusaController.Instance.mapPanel.activeSelf) shouldHide = true;
        }

        if (shouldHide)
        {
            // 如果應該隱藏，就關閉 UI (但保留 currentMessage 資料不清除)
            if (promptText != null && promptText.gameObject.activeSelf) 
                promptText.gameObject.SetActive(false);
                
            if (promptIconImage != null && promptIconImage.gameObject.activeSelf) 
                promptIconImage.gameObject.SetActive(false);
        }
        else
        {
            // 如果不該隱藏，而且原本有留存的文字，就重新顯示出來
            if (!string.IsNullOrEmpty(currentMessage))
            {
                if (promptText != null && !promptText.gameObject.activeSelf)
                {
                    promptText.gameObject.SetActive(true);
                    promptText.text = currentMessage;
                }

                if (promptIconImage != null && currentIcon != null && !promptIconImage.gameObject.activeSelf)
                {
                    promptIconImage.gameObject.SetActive(true);
                    promptIconImage.sprite = currentIcon;
                }
                else if (promptIconImage != null && currentIcon == null && promptIconImage.gameObject.activeSelf)
                {
                    promptIconImage.gameObject.SetActive(false);
                }
            }
        }
    }

    // 支援傳入文字與選填的 Icon
    public void ShowPrompt(string message, Sprite icon = null)
    {
        currentMessage = message;
        currentIcon = icon;
        UpdateVisibility(); // 呼叫時立即更新一次狀態
    }

    // 徹底關閉並清除資料
    public void HidePrompt()
    {
        currentMessage = "";
        currentIcon = null;

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (promptIconImage != null) promptIconImage.gameObject.SetActive(false);
    }

    // ==========================================
    // 🌟 碰撞檢測：偵測 Puzzle Tag
    // ==========================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 當碰到帶有 Puzzle 標籤的物件/面板範圍時，標記為 true，Update 會自動隱藏 Prompt
        if (collision.CompareTag("Puzzle"))
        {
            isNearPuzzle = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 離開 Puzzle 範圍時，解除限制，Update 會自動恢復顯示
        if (collision.CompareTag("Puzzle"))
        {
            isNearPuzzle = false;
        }
    }

    // ==========================================
    // 給 Skill Panel 調用的方法
    // ==========================================
    
    public void SuppressPrompt()
    {
        isSuppressed = true;
        UpdateVisibility();
    }

    public void RestorePrompt()
    {
        isSuppressed = false;
        UpdateVisibility();
    }
}