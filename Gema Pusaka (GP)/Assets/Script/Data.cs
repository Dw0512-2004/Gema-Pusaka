using UnityEngine;
using UnityEngine.SceneManagement;

public class Data : MonoBehaviour
{
    public static Data Instance;

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
        }
    }

    // ==========================================
    // 1. 收集進度與座標 (在存檔點、過場動畫結束時呼叫)
    // ==========================================
    public void SaveGame(int currentProgressPercentage)
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);

        // 儲存當前場景與進度 %
        PlayerPrefs.SetString("Slot_" + activeSlot + "_Scene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Progress", currentProgressPercentage);

        // 儲存玩家當前座標 (如果 Nusa 存在的話)
        if (NusaController.Instance != null)
        {
            PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosX", NusaController.Instance.transform.position.x);
            PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosY", NusaController.Instance.transform.position.y);
            PlayerPrefs.SetInt("Slot_" + activeSlot + "_HasSavedPos", 1);
        }

        PlayerPrefs.Save();
        Debug.Log($"<color=green>【存檔成功】槽位 {activeSlot}，進度 {currentProgressPercentage}%</color>");
    }

    // ==========================================
    // 2. 技能解鎖鏈接 (當玩家撿到樂器/技能時呼叫)
    // ==========================================
    // 0: Serunai, 1: Gong, 2: Gendang, 3: Nobat
    public void UnlockSkill(int skillIndex)
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        
        // 寫入該槽位的永久存檔
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_" + skillIndex, 1);
        
        // 同步更新當前遊戲的 Runtime 狀態，讓玩家立刻可以使用
        PlayerPrefs.SetInt("Runtime_Has_" + GetSkillNameKey(skillIndex), 1);
        
        PlayerPrefs.Save();
        Debug.Log($"<color=cyan>【技能解鎖】成功解鎖技能：{GetSkillNameKey(skillIndex)}</color>");
    }

    // ==========================================
    // 3. 一鍵刪除所有記錄 (核彈按鈕)
    // ==========================================
    public void DeleteAllRecords()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=red>【警告】所有存檔紀錄已被徹底清除！</color>");
    }

    private string GetSkillNameKey(int index)
    {
        switch (index)
        {
            case 0: return "Serunai";
            case 1: return "Gong";
            case 2: return "Gendang";
            case 3: return "Nobat";
            default: return "";
        }
    }
}