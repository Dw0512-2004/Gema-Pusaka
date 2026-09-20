using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI 引用")]
    public GameObject loadingPanel;
    public Slider progressBar;

    private bool isLoading = false; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return; 
        StartCoroutine(LoadSceneCoroutine(sceneName, null));
    }

    public void LoadSceneWithCleanUp(string sceneName, System.Action cleanUpAction)
    {
        if (isLoading) return; 
        StartCoroutine(LoadSceneCoroutine(sceneName, cleanUpAction));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, System.Action cleanUpAction)
    {
        isLoading = true;
        Time.timeScale = 1f;

        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (progressBar != null) progressBar.value = 0f;

        yield return null;

        if (cleanUpAction != null) cleanUpAction.Invoke();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        // 真实加载进度 0.0 -> 0.9 映射为 0 -> 100%
        while (asyncLoad.progress < 0.9f)
        {
            if (progressBar != null) progressBar.value = asyncLoad.progress / 0.9f;
            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;
        asyncLoad.allowSceneActivation = true;

        // 等待 Unity 彻底完成场景切换
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 🚨 核心改动：等待一帧，让新场景的所有物件完成 Awake 初始化
        yield return null;

        // 自动侦测新场景里有没有 GameInitializer
        GameInitializer initializer = Object.FindAnyObjectByType<GameInitializer>();

        if (initializer != null)
        {
            // 情况 A：这是游戏关卡，交给 GameInitializer 处理坐标和相机，它会稍后呼叫 HideLoadingScreen()
            Debug.Log("<color=cyan>【LoadingManager】侦测到 GameInitializer，等待其完成对齐工作...</color>");
        }
        else
        {
            // 情况 B：这是 MainMenu 或 Cutscene，没有玩家需要对齐，直接开场！
            Debug.Log("<color=cyan>【LoadingManager】无 GameInitializer，直接关闭载入画面。</color>");
            HideLoadingScreen();
        }
    }

    // 供 GameInitializer 呼叫，或者在无 GameInitializer 场景自调用的公开方法
    public void HideLoadingScreen()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        isLoading = false;
    }
}