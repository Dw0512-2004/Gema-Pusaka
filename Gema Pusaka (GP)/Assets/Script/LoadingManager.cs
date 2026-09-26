using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("UI 引用")]
    public GameObject loadingPanel;
    public Slider progressBar;
    
    // 🌟 新增：隨機提示文字相關變數
    [Tooltip("用來顯示提示的 Text 元素")]
    public TextMeshProUGUI tipsText;
    
    [Tooltip("載入時隨機播放的句子列表，可以在 Inspector 自由增減")]
    public string[] randomTips = new string[] 
    {
        "記得隨時存檔，以防萬一。",
        "不同的傳統樂器能解開不同的機關。",
        "探索地圖的每個角落，也許會有意外發現！",
        "Nusa 的旅程充滿挑戰，保持耐心。"
    };

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

        // 🌟 遊戲一開始，確保 Panel 和 Text 都被隱藏
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (tipsText != null) tipsText.gameObject.SetActive(false);
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

        // 🌟 在開啟載入畫面之前，隨機抽取一句話顯示
        if (tipsText != null && randomTips != null && randomTips.Length > 0)
        {
            int randomIndex = Random.Range(0, randomTips.Length);
            tipsText.text = randomTips[randomIndex];
            
            // 🌟 強制顯示文字物件
            tipsText.gameObject.SetActive(true);
        }

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
            Debug.Log("<color=cyan>【LoadingManager】侦测到 GameInitializer，等待其完成对齐工作...</color>");
        }
        else
        {
            Debug.Log("<color=cyan>【LoadingManager】无 GameInitializer，直接关闭载入画面。</color>");
            HideLoadingScreen();
        }
    }

    // 供 GameInitializer 呼叫，或者在无 GameInitializer 场景自调用的公开方法
    public void HideLoadingScreen()
    {
        // 🌟 載入結束時，確保 Panel 和 Text 都被關閉
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (tipsText != null) tipsText.gameObject.SetActive(false);
        
        isLoading = false;
    }
}