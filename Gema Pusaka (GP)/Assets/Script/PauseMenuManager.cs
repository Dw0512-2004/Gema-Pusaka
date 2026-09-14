using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [Header("UI 面板引用")]
    [Tooltip("暂停菜单的主面板")]
    public GameObject pauseMenuPanel;
    [Tooltip("设置面板")]
    public GameObject settingsPanel;
    [Tooltip("地图面板 (Map Panel)")]
    public GameObject mapPanel;

    [Header("场景跳转设置")]
    [Tooltip("主菜单的场景名称 (例如 Main_Menu)")]
    public string mainMenuSceneName = "Main_Menu";
    [Tooltip("存档界面的场景名称 (例如 Load_Data_Scene)")]
    public string loadDataSceneName = "Load_Data_Scene";

    [HideInInspector] public bool isPaused = false;

    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 确保游戏开始时所有 UI 菜单都是关闭的
        CloseAllPanels();
    }

    private void Update()
    {
        // 电脑端测试：按 ESC 键呼出/关闭菜单
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // 如果当前在设置面板中，按 ESC 则是返回暂停主菜单
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    OpenPauseMenuOnly();
                }
                // 如果当前在地图面板中，按 ESC 则是直接回到游戏
                else if (mapPanel != null && mapPanel.activeSelf)
                {
                    CloseMap();
                }
                // 如果在暂停主菜单中，按 ESC 也是回到游戏
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ==========================================
    // 供 UI 按钮调用的公开方法
    // ==========================================

    // 1. 暂停游戏 (呼出暂停主菜单)
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 冻结游戏时间
        
        // 禁用玩家移动和输入
        if (NusaController.Instance != null)
        {
            NusaController.Instance.DisableMovement();
        }

        OpenPauseMenuOnly();
    }

    // 2. 继续游戏 (Resume)
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 恢复游戏时间
        
        // 恢复玩家移动
        if (NusaController.Instance != null)
        {
            NusaController.Instance.EnableMovement();
        }

        CloseAllPanels();
    }

    // 3. 打开设置面板 (Settings)
    public void OpenSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // ==========================================
    // 地图专属方法 (Map)
    // ==========================================

    // 打开地图 (绑定给游戏主界面的 Map Button)
    public void OpenMap()
    {
        isPaused = true;
        Time.timeScale = 0f; // 冻结游戏时间
        
        if (NusaController.Instance != null)
        {
            NusaController.Instance.DisableMovement();
        }

        // 隐藏其他面板，只显示地图
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(true);

        Debug.Log("<color=cyan>打开地图，游戏已暂停</color>");
    }

    // 关闭地图 (绑定给地图面板里的 Back/Close 按钮)
    public void CloseMap()
    {
        // 直接复用恢复游戏的逻辑即可，它会自动关闭所有面板并恢复时间
        ResumeGame();
        Debug.Log("<color=cyan>关闭地图，游戏已恢复</color>");
    }

    // ==========================================
    // 存档与跳转方法
    // ==========================================

    // 4. 【核心存档】保存当前游戏状态，并直接回到游戏 (绑定给 Save Data 按钮)
    public void SaveGameDataToCurrentSlot()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("Slot_" + activeSlot + "_Scene", currentScene);

        int currentProgress = CalculateCurrentProgress(); 
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Progress", currentProgress);

        SaveSkillStates(activeSlot);

        if (NusaController.Instance != null)
        {
            PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosX", NusaController.Instance.transform.position.x);
            PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosY", NusaController.Instance.transform.position.y);
            PlayerPrefs.SetInt("Slot_" + activeSlot + "_HasSavedPos", 1);
        }

        PlayerPrefs.Save();
        Debug.Log($"<color=green>【快速存档成功】所有数据及位置已保存至槽位 {activeSlot}！当前场景: {currentScene}</color>");

        ResumeGame();
    }

    // 5. 跳转到存档场景 (独立按钮使用)
    public void GoToLoadDataScene()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 0);
        PlayerPrefs.Save();
        SafeExitToScene(loadDataSceneName);
    }

    // 6. 退出到主菜单 (Exit)
    public void ExitToMainMenu()
    {
        SafeExitToScene(mainMenuSceneName);
    }

    // ==========================================
    // 辅助存档数据收集方法
    // ==========================================

    private int CalculateCurrentProgress()
    {
        int progress = PlayerPrefs.GetInt("Runtime_GameProgress", 0); 
        return progress == 0 ? 10 : progress; 
    }

    private void SaveSkillStates(int slotIndex)
    {
        int serunaiUnlocked = PlayerPrefs.GetInt("Runtime_Has_Serunai", 0);
        int gongUnlocked = PlayerPrefs.GetInt("Runtime_Has_Gong", 0);
        int gendangUnlocked = PlayerPrefs.GetInt("Runtime_Has_Gendang", 0);
        int nobatUnlocked = PlayerPrefs.GetInt("Runtime_Has_Nobat", 0);

        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Skill_0", serunaiUnlocked);
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Skill_1", gongUnlocked);
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Skill_2", gendangUnlocked);
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Skill_3", nobatUnlocked);
    }

    // ==========================================
    // 内部核心逻辑
    // ==========================================

    // 只显示暂停主菜单，隐藏其他子面板
    public void OpenPauseMenuOnly()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    // 关闭所有面板
    private void CloseAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    // 终极防宕机：安全清理与带 Loading 动画的跳转逻辑
    private void SafeExitToScene(string targetSceneName)
    {
        // 将所有清理逻辑打包成一个“动作 (Action)”
        System.Action cleanUpLogic = () => 
        {
            Time.timeScale = 1f; 
            isPaused = false;
            CloseAllPanels();

            GameObject playerObj = NusaController.Instance != null ? NusaController.Instance.gameObject : null;
            GameObject coreObj = GameObject.Find("CoreManager");
            if (coreObj == null) coreObj = transform.root.gameObject; 

            NusaController.Instance = null;
            PauseMenuManager.Instance = null;
            SkillManager.Instance = null; 

            Scene currentScene = SceneManager.GetActiveScene();
            
            if (playerObj != null) SceneManager.MoveGameObjectToScene(playerObj, currentScene);
            if (coreObj != null) SceneManager.MoveGameObjectToScene(coreObj, currentScene);

            GameObject skillManagerObj = GameObject.Find("[System_SkillManager]");
            if (skillManagerObj != null) SceneManager.MoveGameObjectToScene(skillManagerObj, currentScene);
        };

        // 呼叫全局 LoadingManager，先弹出加载面板，在黑屏掩护下执行清理，然后加载新场景！
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadSceneWithCleanUp(targetSceneName, cleanUpLogic);
        }
        else
        {
            // 备用：万一没挂 LoadingManager，依然能硬切场景不报错
            Debug.LogWarning("未找到 LoadingManager，将直接跳转场景。");
            cleanUpLogic.Invoke();
            SceneManager.LoadScene(targetSceneName);
        }
    }
}