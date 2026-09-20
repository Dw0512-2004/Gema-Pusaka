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
        if (promptText != null)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }

        // 如果有指定 Icon 圖片，就顯示出來；沒有就關閉 Image
        if (promptIconImage != null)
        {
            if (icon != null)
            {
                promptIconImage.sprite = icon;
                promptIconImage.gameObject.SetActive(true);
            }
            else
            {
                promptIconImage.gameObject.SetActive(false);
            }
        }
    }

    public void HidePrompt()
    {
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
}