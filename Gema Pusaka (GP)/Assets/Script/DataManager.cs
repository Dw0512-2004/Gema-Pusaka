using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // 引入 TextMeshPro 命名空间

public class DataSceneManager : MonoBehaviour
{
    [Header("场景跳转设置")]
    [Tooltip("如果是新游戏，默认进入的第一个剧情/关卡场景 (例如 Scene1_Cutscene 或 tutorial_scene)")]
    public string defaultStartSceneName = "Scene1_Cutscene"; 

    [System.Serializable]
    public class SaveSlotUI
    {
        [Header("基础交互")]
        public Button slotButton;

        [Header("状态容器 (用于控制显隐)")]
        public GameObject emptyStateObject;   // 空存档时显示的物体
        public GameObject usedStateObject;    // 已使用时显示的物体

        [Header("已使用状态下的 TMP 文本")]
        public TextMeshProUGUI sceneNameText; // 显示当前所在场景的名字
        public TextMeshProUGUI progressText;  // 显示游戏进度

        [Header("技能解锁图标 (Icon)")]
        [Tooltip("顺序对应：0-Serunai, 1-Gong, 2-Gendang, 3-Nobat")]
        public Image[] skillIcons = new Image[4]; 
        
        [Header("图标颜色设置")]
        public Color unlockedColor = Color.white;             // 解锁后的明亮颜色
        public Color lockedColor = new Color(1, 1, 1, 0.2f); // 未解锁时的半透明/暗色
    }

    [Header("4个存档槽位 UI 配置")]
    public SaveSlotUI[] saveSlots = new SaveSlotUI[4];

    private void Start()
    {
        RefreshSlots();
    }

    // 刷新显示所有 4 个槽位的状态
    private void RefreshSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slotIndex = i + 1; // 槽位 1, 2, 3, 4
            
            bool exists = PlayerPrefs.GetInt("Slot_" + slotIndex + "_Exists", 0) == 1;
            SaveSlotUI slot = saveSlots[i];

            if (exists)
            {
                // --- 已被使用状态 ---
                if (slot.emptyStateObject != null) slot.emptyStateObject.SetActive(false);
                if (slot.usedStateObject != null) slot.usedStateObject.SetActive(true);

                // 1. 读取并显示场景名
                string sceneName = PlayerPrefs.GetString("Slot_" + slotIndex + "_Scene", "未知区域");
                if (slot.sceneNameText != null) slot.sceneNameText.text = "Scene: " + sceneName;

                // 2. 读取并显示游戏进度
                int progress = PlayerPrefs.GetInt("Slot_" + slotIndex + "_Progress", 0);
                if (slot.progressText != null) slot.progressText.text = "Progress: " + progress + "%";

                // 3. 读取并更新 4 个技能的 Icon 状态
                for (int s = 0; s < slot.skillIcons.Length; s++)
                {
                    if (slot.skillIcons[s] != null)
                    {
                        int isUnlocked = PlayerPrefs.GetInt("Slot_" + slotIndex + "_Skill_" + s, 0);
                        slot.skillIcons[s].color = (isUnlocked == 1) ? slot.unlockedColor : slot.lockedColor;
                    }
                }
            }
            else
            {
                // --- 空白未用状态 ---
                if (slot.emptyStateObject != null) slot.emptyStateObject.SetActive(true);
                if (slot.usedStateObject != null) slot.usedStateObject.SetActive(false);
            }

            // 绑定按钮点击事件
            int indexClosure = slotIndex; 
            slot.slotButton.onClick.RemoveAllListeners();
            slot.slotButton.onClick.AddListener(() => OnSlotClicked(indexClosure, exists));
        }
    }

    // 当玩家点击某个槽位时触发
    private void OnSlotClicked(int slotIndex, bool exists)
    {
        int intent = PlayerPrefs.GetInt("IsNewGame_Intent", 0);

        if (intent == 1) 
        {
            CreateNewSave(slotIndex);
        }
        else 
        {
            if (exists)
            {
                LoadSave(slotIndex);
            }
            else
            {
                CreateNewSave(slotIndex);
            }
        }
    }

    // 创建新存档
    private void CreateNewSave(int slotIndex)
    {
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Exists", 1);
        PlayerPrefs.SetString("Slot_" + slotIndex + "_Scene", defaultStartSceneName);
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Progress", 0);
        
        // 初始化技能...
        for (int s = 0; s < 4; s++)
        {
            PlayerPrefs.SetInt("Slot_" + slotIndex + "_Skill_" + s, 0);
        }

        // 清空坐标标记，确保新游戏从头开始
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_HasSavedPos", 0); 

        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();
        
        // 直接使用 LoadingManager 过渡跳转到新游戏的第一幕 (例如 Scene1_Cutscene)
        SafeLoadScene(defaultStartSceneName);
    }

    // 读取已有存档
    private void LoadSave(int slotIndex)
    {
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();

        // 读取该存档里记录的目标场景名字
        string targetScene = PlayerPrefs.GetString("Slot_" + slotIndex + "_Scene", defaultStartSceneName);

        // 使用 LoadingManager 丝滑跳转到存档记录的场景
        SafeLoadScene(targetScene);
    }

    // 返回主菜单按钮方法
    public void BackToMainMenu()
    {
        SafeLoadScene("Main_Menu"); // 请确保你的主菜单场景名称和这里一致
    }

    // 内部安全加载方法
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