using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    // 全局唯一的单例
    public static LoadingManager Instance;

    [Header("UI 引用")]
    [Tooltip("拖入整个 Loading 面板的父物体")]
    public GameObject loadingPanel;
    
    [Tooltip("拖入进度条 (Slider 组件)")]
    public Slider progressBar;

    [Header("加载设置")]
    [Tooltip("固定加载过渡时间（秒）")]
    public float loadingDuration = 5.0f;

    [Header("状态标记")]
    private bool isLoading = false; // 防止重复触发加载

    private void Awake()
    {
        // 1. 打造绝对不死的单例
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

        // 2. 游戏启动时确保 Loading 界面是隐藏的
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    // ==========================================
    // 普通的场景切换方法
    // ==========================================
    public void LoadScene(string sceneName)
    {
        if (isLoading) return; 
        StartCoroutine(LoadSceneCoroutine(sceneName, null));
    }

    // ==========================================
    // 专为“退出/返回”设计的带清理逻辑的切换方法
    // ==========================================
    public void LoadSceneWithCleanUp(string sceneName, System.Action cleanUpAction)
    {
        if (isLoading) return; 
        StartCoroutine(LoadSceneCoroutine(sceneName, cleanUpAction));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, System.Action cleanUpAction)
    {
        isLoading = true;

        // 1. 确保时间恢复正常，显示加载面板，重置进度条
        Time.timeScale = 1f;
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (progressBar != null) progressBar.value = 0f;

        yield return null;

        // 2. 执行传入的清理逻辑（销毁旧单例等）
        if (cleanUpAction != null)
        {
            cleanUpAction.Invoke();
        }

        // 3. 异步加载目标场景，禁止自动激活
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        // 4. 固定的 5 秒平滑进度条动画（绝对不会卡死）
        float timer = 0f;
        while (timer < loadingDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / loadingDuration);
            
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            yield return null;
        }

        // 5. 5 秒时间到，进度条稳稳拉满到 100%
        if (progressBar != null) progressBar.value = 1f;

        // 6. 允许 Unity 正式激活跳转新场景
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 7. 【核心保护期】：此时新场景刚加载完，但黑屏（loadingPanel）还在！
        // 我们立刻在黑屏保护下对齐玩家坐标与相机，玩家绝对不会看到任何闪烁或穿帮。
        InitializePlayerAndCamera();

        // 等待一帧确保画面渲染稳定
        yield return null; 

        // 8. 完美收兵：隐藏 Loading 面板
        if (loadingPanel != null) loadingPanel.SetActive(false);
        
        isLoading = false;
    }

    // 在黑屏掩护下初始化玩家坐标、物理状态与相机
    private void InitializePlayerAndCamera()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        bool hasSavedPos = PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1;

        if (NusaController.Instance != null)
        {
            NusaController.Instance.canMove = false;
            Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // 恢复位置
            if (hasSavedPos)
            {
                float posX = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosX", 0f);
                float posY = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosY", 0f);
                NusaController.Instance.transform.position = new Vector3(posX, posY, 0f);
            }
            else
            {
                GameObject spawnPoint = GameObject.Find("Spawn_Start");
                if (spawnPoint == null) spawnPoint = GameObject.Find("SpawnPoint");

                if (spawnPoint != null)
                {
                    NusaController.Instance.transform.position = spawnPoint.transform.position;
                }
            }

            // 绑定相机
            CinemachineCamera vcam = Object.FindAnyObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                vcam.Follow = NusaController.Instance.transform;
                vcam.OnTargetObjectWarped(NusaController.Instance.transform, NusaController.Instance.transform.position - vcam.transform.position);
            }

            // 恢复控制与时间
            NusaController.Instance.canMove = true;
            Time.timeScale = 1f;
        }
    }
}