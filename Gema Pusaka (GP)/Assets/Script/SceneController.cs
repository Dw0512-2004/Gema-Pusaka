using UnityEngine;

public class Scene1Controller : MonoBehaviour
{
    [Header("跳转设置")]
    [Tooltip("Scene 2 的场景名称（例如 Scene2_Lab）")]
    public string nextSceneName = "Scene2_Lab"; // 对应你接下来要做的 Scene 2 场景名

    /// <summary>
    /// 供 Fungus 的 Call Method 调用的方法（在最后一幕结束时触发）
    /// </summary>
    public void ProceedToTutorial()
    {
        Debug.Log("<color=green>【Scene 1 结束】准备通过 LoadingManager 切换到 Scene 2: " + nextSceneName + "</color>");

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(nextSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}