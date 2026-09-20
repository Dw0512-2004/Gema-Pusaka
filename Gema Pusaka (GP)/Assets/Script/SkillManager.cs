using UnityEngine;
using System;
using UnityEngine.UI; // 加入以使用 Button

[RequireComponent(typeof(AudioSource))]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("UI 引用")]
    [Tooltip("拖入包含所有按钮和背景的父级 Panel_Background")]
    public GameObject skillPanelUI;

    [Header("技能按钮引用 (用于控制显示/隐藏)")]
    [Tooltip("手机端的虚拟互动/技能施放按钮")]
    public GameObject interactButton; // 新增：控制主互动按钮的显示
    public GameObject btnSerunai;
    public GameObject btnGong;
    public GameObject btnGendang;
    public GameObject btnNobat;

    [Header("音效设置 (Audio Clips)")]
    public AudioClip serunaiSound;
    public AudioClip gongSound;
    public AudioClip gendangSound;
    public AudioClip nobatSound;

    private AudioSource audioSource;

    // --- 定义技能枚举与当前状态 ---
    public enum SkillType { None, Serunai, Gong, Gendang, Nobat }
    
    [Header("当前装备的技能")]
    public SkillType currentSkill = SkillType.None;

    [Header("技能解锁状态 (Fungus 激活)")]
    public bool isSerunaiUnlocked = false;
    public bool isGongUnlocked = false;
    public bool isGendangUnlocked = false;
    public bool isNobatUnlocked = false;

    // --- 定义全局技能事件 (携带音频播放时长) ---
    public static event Action<float> OnSerunaiUsed;  
    public static event Action<float> OnGongUsed;     
    public static event Action<float> OnGendangUsed;  
    public static event Action<float> OnNobatUsed;    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
        
        HidePanel();
    }

    private void Start()
    {
        // 游戏启动时刷新一次按钮显示状态
        UpdateSkillButtonsVisibility();
    }

    // ==========================================
    // ▼ 供 Fungus 调用的解锁方法 ▼
    // ==========================================

    public void UnlockSerunai()
    {
        isSerunaiUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Serunai", 1);
        Debug.Log("<color=yellow>【技能解锁】获得技能：Serunai！</color>");
        UpdateSkillButtonsVisibility();
        
        // 如果当前没有装备技能，自动装备刚解锁的这个
        if (currentSkill == SkillType.None) currentSkill = SkillType.Serunai;
    }

    public void UnlockGong()
    {
        isGongUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Gong", 1);
        Debug.Log("<color=yellow>【技能解锁】获得技能：Gong！</color>");
        UpdateSkillButtonsVisibility();
        if (currentSkill == SkillType.None) currentSkill = SkillType.Gong;
    }

    public void UnlockGendang()
    {
        isGendangUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Gendang", 1);
        Debug.Log("<color=yellow>【技能解锁】获得技能：Gendang！</color>");
        UpdateSkillButtonsVisibility();
        if (currentSkill == SkillType.None) currentSkill = SkillType.Gendang;
    }

    public void UnlockNobat()
    {
        isNobatUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Nobat", 1);
        Debug.Log("<color=yellow>【技能解锁】获得技能：Nobat！</color>");
        UpdateSkillButtonsVisibility();
        if (currentSkill == SkillType.None) currentSkill = SkillType.Nobat;
    }

    // 判断玩家是否解锁了"至少一个"技能
    public bool HasAnySkillUnlocked()
    {
        return isSerunaiUnlocked || isGongUnlocked || isGendangUnlocked || isNobatUnlocked;
    }

    // 更新面板上按钮的显示状态
    public void UpdateSkillButtonsVisibility()
    {
        // 1. 只要有任何一个技能解锁，就显示右下角的互动/技能按钮
        if (interactButton != null) interactButton.SetActive(HasAnySkillUnlocked());

        // 2. 更新面板内个别技能的按钮
        if (btnSerunai != null) btnSerunai.SetActive(isSerunaiUnlocked);
        if (btnGong != null) btnGong.SetActive(isGongUnlocked);
        if (btnGendang != null) btnGendang.SetActive(isGendangUnlocked);
        if (btnNobat != null) btnNobat.SetActive(isNobatUnlocked);
    }

    // ==========================================
    // 面板控制
    // ==========================================

    public void ShowPanel()
    {
        // 【关键防御】：如果一个技能都没解锁，强制拒绝呼出面板
        if (!HasAnySkillUnlocked())
        {
            Debug.Log("未解锁任何技能，无法打开技能面板。");
            return;
        }

        UpdateSkillButtonsVisibility(); // 确保打开前更新一下

        if (skillPanelUI != null) skillPanelUI.SetActive(true);
        
        if (NusaController.Instance != null)
        {
            NusaController.Instance.DisableMovement();
        }
    }

    public void HidePanel()
    {
        if (skillPanelUI != null) skillPanelUI.SetActive(false);
        
        if (NusaController.Instance != null)
        {
            NusaController.Instance.EnableMovement();
        }
    }

    // ==========================================
    // 供 UI 面板按钮 OnClick 调用的方法
    // ==========================================
    public void SelectSkill_Serunai() { currentSkill = SkillType.Serunai; HidePanel(); }
    public void SelectSkill_Gong()    { currentSkill = SkillType.Gong;    HidePanel(); }
    public void SkillSelect_Gendang() { currentSkill = SkillType.Gendang; HidePanel(); } 
    public void SelectSkill_Gendang() { currentSkill = SkillType.Gendang; HidePanel(); }
    public void SelectSkill_Nobat()   { currentSkill = SkillType.Nobat;   HidePanel(); }

    // ==========================================
    // 供 NusaController 短按调用的方法
    // ==========================================
    public void CastCurrentSkill()
    {
        // 【关键防御】：如果没有解锁任何技能，直接返回
        if (!HasAnySkillUnlocked() || currentSkill == SkillType.None)
        {
            Debug.Log("未装备或未解锁任何技能，无法释放。");
            return;
        }

        float duration = 0f;
        switch (currentSkill)
        {
            case SkillType.Serunai:
                duration = serunaiSound != null ? serunaiSound.length : 0f;
                PlaySound(serunaiSound);
                OnSerunaiUsed?.Invoke(duration);
                break;
                
            case SkillType.Gong:
                duration = gongSound != null ? gongSound.length : 0f;
                PlaySound(gongSound);
                OnGongUsed?.Invoke(duration);
                break;
                
            case SkillType.Gendang:
                duration = gendangSound != null ? gendangSound.length : 0f;
                PlaySound(gendangSound);
                OnGendangUsed?.Invoke(duration);
                break;
                
            case SkillType.Nobat:
                duration = nobatSound != null ? nobatSound.length : 0f;
                PlaySound(nobatSound);
                OnNobatUsed?.Invoke(duration);
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}