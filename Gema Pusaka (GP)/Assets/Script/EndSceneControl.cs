using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneControl : MonoBehaviour
{
    [Header("場景設定")]
    [Tooltip("請輸入你要跳轉到的結尾場景名稱 (例如：ModernLabScene)")]
    public string targetSceneName;

    // 提供給 Fungus 呼叫的方法
    public void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"<color=green>【場景跳轉】準備切換至場景: {targetSceneName}</color>");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("【錯誤】未設定目標場景名稱！");
        }
    }
}