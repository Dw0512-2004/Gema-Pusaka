using UnityEngine;
using Fungus; // 必須引入 Fungus 命名空間才能控制 Flowchart

public class StatueSkillManager : MonoBehaviour
{
    [Header("Fungus 設定")]
    public Flowchart flowchart; // 在 Inspector 中把場景上的 Flowchart 拉進來
    public string allUnlockedBlockName = "AllSkillsUnlocked"; // 達成條件時要呼叫的 Block 名稱
    public string notUnlockedBlockName = "SayNotEnough";      // 沒達成時要呼叫的 Block 名稱 (裡面放 Say 指令)

    // ==========================================
    // 這些方法用來給 Fungus 的 Invoke Method 呼叫
    // 它們會自動尋找目前場景中的 SkillManager 單例
    // ==========================================

    public void GiveSerunai()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.UnlockSerunai();
        }
        else
        {
            Debug.LogError("找不到 SkillManager！請確定 Nusa 存在於場景中。");
        }
    }

    public void GiveGong()
    {
        if (SkillManager.Instance != null) SkillManager.Instance.UnlockGong();
    }

    public void GiveGendang()
    {
        if (SkillManager.Instance != null) SkillManager.Instance.UnlockGendang();
    }

    public void GiveNobat()
    {
        if (SkillManager.Instance != null) SkillManager.Instance.UnlockNobat();
    }

    // ==========================================
    // 新增：檢查三個技能是否解鎖，並引導 Fungus
    // ==========================================
    public void CheckSkillsAndTriggerFungus()
    {
        if (SkillManager.Instance == null)
        {
            Debug.LogError("找不到 SkillManager！");
            return;
        }

        if (flowchart == null)
        {
            Debug.LogError("請在 Inspector 中綁定 Flowchart！");
            return;
        }

        // ⚠️ 請將這裡的 isSerunaiUnlocked, isGongUnlocked, isGendangUnlocked 
        // 替換成你 SkillManager 裡面實際用來記錄解鎖狀態的 bool 變數或方法
        bool hasSkill1 = SkillManager.Instance.isSerunaiUnlocked;
        bool hasSkill2 = SkillManager.Instance.isGongUnlocked;
        bool hasSkill3 = SkillManager.Instance.isGendangUnlocked;

        // 如果三個技能都解鎖了
        if (hasSkill1 && hasSkill2 && hasSkill3)
        {
            // 呼叫進入下一個階段的 Block
            flowchart.ExecuteBlock(allUnlockedBlockName);
        }
        else
        {
            // 條件未滿，呼叫用來彈出對話框 (Say) 拒絕玩家的 Block
            flowchart.ExecuteBlock(notUnlockedBlockName);
        }
    }
}