using UnityEngine;

public class IntroSceneController : MonoBehaviour
{
    [Header("开场剧情播完后要进入的目标关卡")]
    [Tooltip("填入你游戏第一关的场景名称")]
    public string nextSceneName = "tutorial_scene";

    /// <summary>
    /// 供 Fungus 的 Call Method 调用的方法
    /// </summary>
    public void ProceedToNextScene()
    {
        if (LoadingManager.Instance != null)
        {
            // 呼叫全局 LoadingManager，启动 5 秒平滑过渡和进度条！
            LoadingManager.Instance.LoadScene(nextSceneName);
        }
        else
        {
            // 保底机制
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}