using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 面板引用")]
    [Tooltip("主菜单按钮的父物体（包含 New Game, Load Game, Settings, Exit）")]
    public GameObject mainButtonsContainer; 
    [Tooltip("设置面板（Settings Panel）")]
    public GameObject settingsPanel;

    [Header("场景跳转设置")]
    [Tooltip("存档界面的场景名称")]
    public string loadDataSceneName = "Load_Data_Scene";

    private void Start()
    {
        // 游戏刚启动时，确保设置面板是隐藏的，主按钮是显示的
        CloseSettings();
    }

    // ==========================================
    // 供 UI 按钮调用的公开方法
    // ==========================================

    // 1. 点击 New Game 按钮
    public void OnNewGameClicked()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 1);
        PlayerPrefs.Save();
        
        // 【修改点】：使用 LoadingManager 过渡跳转到存档选择界面
        SafeLoadScene(loadDataSceneName);
    }

    // 2. 点击 Load Game 按钮
    public void OnLoadGameClicked()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 0);
        PlayerPrefs.Save();
        
        // 【修改点】：使用 LoadingManager 过渡跳转到存档选择界面
        SafeLoadScene(loadDataSceneName);
    }

    // 3. 点击主菜单的 Settings 按钮
    public void OpenSettings()
    {
        // 隐藏主菜单按钮，显示设置面板
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // 4. 点击设置面板里的 Back 按钮
    public void CloseSettings()
    {
        // 隐藏设置面板，重新显示主菜单按钮
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainButtonsContainer != null) mainButtonsContainer.SetActive(true);
    }

    // 5. 点击 Exit 按钮
    public void ExitGame()
    {
        Debug.Log("退出游戏！(打包后生效)");
        Application.Quit();
    }

    // ==========================================
    // 内部安全加载方法：优先调用 LoadingManager
    // ==========================================
    private void SafeLoadScene(string sceneName)
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}