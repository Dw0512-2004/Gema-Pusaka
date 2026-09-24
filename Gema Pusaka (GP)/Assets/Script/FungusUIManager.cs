using UnityEngine;
using Fungus;

public class FungusUIManager : MonoBehaviour
{
    // ==========================================
    // 这些方法供 Fungus 的 Invoke Method 调用
    // ==========================================

    public void HideNusaUI()
    {
        // 直接通过单例访问当前的 Nusa
        if (NusaController.Instance != null)
        {
            NusaController.Instance.HideUIForDialogue();
            Debug.Log("FungusUIManager: 已通知 Nusa 隐藏 UI 并锁定移动。");
        }
        else
        {
            Debug.LogWarning("FungusUIManager: 找不到 NusaController.Instance，无法隐藏 UI！");
        }
    }

    public void ShowNusaUI()
    {
        if (NusaController.Instance != null)
        {
            NusaController.Instance.ShowUIAfterDialogue();
            Debug.Log("FungusUIManager: 已通知 Nusa 恢复 UI 并允许移动。");
        }
        else
        {
            Debug.LogWarning("FungusUIManager: 找不到 NusaController.Instance，无法恢复 UI！");
        }
    }
}