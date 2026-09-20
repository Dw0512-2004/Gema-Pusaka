using UnityEngine;

public class StatueSkillManager : MonoBehaviour
{
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
}