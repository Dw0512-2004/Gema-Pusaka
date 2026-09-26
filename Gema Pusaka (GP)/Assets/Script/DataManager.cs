using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    [Header("場景跳轉設置")]
    public string defaultStartSceneName = "Introduction Scene"; 

    [Header("Prefab 與生成設置")]
    public GameObject saveSlotPrefab;
    [Tooltip("請拖入 Scroll View -> Viewport -> Content")]
    public Transform slotsContainer;

    [Header("槽位數量")]
    public int totalSlots = 10; 

    private void Start()
    {
        GenerateSlots();
    }

    private void GenerateSlots()
    {
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < totalSlots; i++)
        {
            int slotIndex = i + 1; 
            GameObject newSlotObj = Instantiate(saveSlotPrefab, slotsContainer);
            
            SaveSlotUI slotUI = newSlotObj.GetComponent<SaveSlotUI>();
            if (slotUI != null)
            {
                slotUI.Initialize(slotIndex, this);
            }
        }
    }

    public void OnSlotClicked(int slotIndex, bool exists)
    {
        int intent = PlayerPrefs.GetInt("IsNewGame_Intent", 0);

        if (intent == 1) 
            CreateNewSave(slotIndex);
        else if (exists)
            LoadSave(slotIndex);
        else
            CreateNewSave(slotIndex);
    }

    private void CreateNewSave(int slotIndex)
    {
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Exists", 1);
        PlayerPrefs.SetString("Slot_" + slotIndex + "_Scene", defaultStartSceneName);
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_Progress", 0);
        
        for (int s = 0; s < 4; s++)
        {
            PlayerPrefs.SetInt("Slot_" + slotIndex + "_Skill_" + s, 0);
        }
        PlayerPrefs.SetInt("Slot_" + slotIndex + "_HasSavedPos", 0); 
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        
        // 🌟 創建新存檔時，清空 Runtime 數據
        PlayerPrefs.SetInt("Runtime_Has_Serunai", 0);
        PlayerPrefs.SetInt("Runtime_Has_Gong", 0);
        PlayerPrefs.SetInt("Runtime_Has_Gendang", 0);
        PlayerPrefs.SetInt("Runtime_Has_Nobat", 0);
        PlayerPrefs.SetInt("Runtime_GameProgress", 0);

        PlayerPrefs.Save();
        SafeLoadScene(defaultStartSceneName);
    }

    private void LoadSave(int slotIndex)
    {
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        
        // 🌟 載入存檔時，將 Slot 的數據覆寫回 Runtime 變數
        PlayerPrefs.SetInt("Runtime_Has_Serunai", PlayerPrefs.GetInt("Slot_" + slotIndex + "_Skill_0", 0));
        PlayerPrefs.SetInt("Runtime_Has_Gong", PlayerPrefs.GetInt("Slot_" + slotIndex + "_Skill_1", 0));
        PlayerPrefs.SetInt("Runtime_Has_Gendang", PlayerPrefs.GetInt("Slot_" + slotIndex + "_Skill_2", 0));
        PlayerPrefs.SetInt("Runtime_Has_Nobat", PlayerPrefs.GetInt("Slot_" + slotIndex + "_Skill_3", 0));
        PlayerPrefs.SetInt("Runtime_GameProgress", PlayerPrefs.GetInt("Slot_" + slotIndex + "_Progress", 0));

        PlayerPrefs.Save();
        
        string targetScene = PlayerPrefs.GetString("Slot_" + slotIndex + "_Scene", defaultStartSceneName);
        SafeLoadScene(targetScene);
    }

    public void BackToMainMenu() => SafeLoadScene("Main Menu"); 

    private void SafeLoadScene(string sceneName)
    {
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}