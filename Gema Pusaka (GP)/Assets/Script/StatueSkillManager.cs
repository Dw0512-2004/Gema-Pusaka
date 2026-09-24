using UnityEngine;
using Fungus; 

public class StatueSkillManager : MonoBehaviour
{
    [Header("Fungus 設定")]
    public Flowchart flowchart; 

    // ==========================================
    // 新增：給 Fungus 呼叫的方法，用來更新 Fungus 裡的變數
    // ==========================================
    public void CheckSkillsForFungus()
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

        bool hasSkill1 = SkillManager.Instance.isSerunaiUnlocked;
        bool hasSkill2 = SkillManager.Instance.isGongUnlocked;
        bool hasSkill3 = SkillManager.Instance.isGendangUnlocked;

        // 判斷是否三個都解鎖了
        bool allUnlocked = hasSkill1 && hasSkill2 && hasSkill3;

        // 將結果寫入 Fungus 中的布林變數 "CanUnlockNobat"
        flowchart.SetBooleanVariable("CanUnlockNobat", allUnlocked);
    }

    // ==========================================
    // 保留您原本解鎖技能的方法
    // ==========================================
    public void GiveSerunai()
    {
        if (SkillManager.Instance != null) SkillManager.Instance.UnlockSerunai();
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
}