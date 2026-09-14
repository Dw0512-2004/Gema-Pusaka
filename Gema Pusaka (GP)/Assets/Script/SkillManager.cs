using UnityEngine;
using System; // 必须引入此命名空间以使用 Action

[RequireComponent(typeof(AudioSource))]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("UI 引用")]
    [Tooltip("拖入包含所有按钮和背景的父级 Panel_Background")]
    public GameObject skillPanelUI; 

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

    // --- 定义全局技能事件 (携带音频播放时长) ---
    public static event Action<float> OnSerunaiUsed;  
    public static event Action<float> OnGongUsed;     
    public static event Action<float> OnGendangUsed;  
    public static event Action<float> OnNobatUsed;    

    private void Awake()
    {
        // 核心管理器单例设置
        if (Instance == null)
        {
            Instance = this;
            
            // 【终极加固】：如果这个管理器没有父物体（是个独立物体），强制让它跨场景存活！
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
        
        // 游戏开始时确保面板是隐藏的
        HidePanel();
    }

    // 呼出面板 (由 NusaController 的长按逻辑触发)
    public void ShowPanel()
    {
        if (skillPanelUI != null) skillPanelUI.SetActive(true);
        
        // 呼出面板时，锁死玩家移动
        if (NusaController.Instance != null)
        {
            NusaController.Instance.DisableMovement();
        }
    }

    // 隐藏面板
    public void HidePanel()
    {
        if (skillPanelUI != null) skillPanelUI.SetActive(false);
        
        // 恢复玩家移动
        if (NusaController.Instance != null)
        {
            NusaController.Instance.EnableMovement();
        }
    }

    // ==========================================
    // 供 UI 面板按钮 OnClick 调用的方法：只负责“选择”并隐藏面板
    // ==========================================
    public void SelectSkill_Serunai() { currentSkill = SkillType.Serunai; HidePanel(); }
    public void SelectSkill_Gong()    { currentSkill = SkillType.Gong;    HidePanel(); }
    public void SkillSelect_Gendang() { currentSkill = SkillType.Gendang; HidePanel(); } // 保持兼容
    public void SelectSkill_Gendang() { currentSkill = SkillType.Gendang; HidePanel(); }
    public void SelectSkill_Nobat()   { currentSkill = SkillType.Nobat;   HidePanel(); }

    // ==========================================
    // 供 NusaController 短按调用的方法：执行“释放”
    // ==========================================
    public void CastCurrentSkill()
    {
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
                
            case SkillType.None:
                Debug.Log("未装备任何技能，无法释放。请长按交互键选择技能。");
                break;
        }
    }

    // 播放对应乐器音效
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}