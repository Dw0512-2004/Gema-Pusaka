using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("UI 引用")]
    public GameObject skillPanelUI;

    [Header("技能按钮引用")]
    public GameObject interactButton; 
    public GameObject btnSerunai;
    public GameObject btnGong;
    public GameObject btnGendang;
    public GameObject btnNobat;

    [Header("音效设置")]
    public AudioClip serunaiSound;
    public AudioClip gongSound;
    public AudioClip gendangSound;
    public AudioClip nobatSound;

    private AudioSource audioSource;

    public enum SkillType { None, Serunai, Gong, Gendang, Nobat }
    
    [Header("当前装备的技能")]
    public SkillType currentSkill = SkillType.None;

    [Header("技能解锁状态")]
    public bool isSerunaiUnlocked = false;
    public bool isGongUnlocked = false;
    public bool isGendangUnlocked = false;
    public bool isNobatUnlocked = false;

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

    // 🌟 遊戲啟動時，強制讀取一次
    private void Start()
    {
        SyncSkillsFromRuntime();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 場景切換時，確保同步一次
        SyncSkillsFromRuntime();
    }

    // 🌟 統一的數據同步方法
    private void SyncSkillsFromRuntime()
    {
        isSerunaiUnlocked = PlayerPrefs.GetInt("Runtime_Has_Serunai", 0) == 1;
        isGongUnlocked = PlayerPrefs.GetInt("Runtime_Has_Gong", 0) == 1;
        isGendangUnlocked = PlayerPrefs.GetInt("Runtime_Has_Gendang", 0) == 1;
        isNobatUnlocked = PlayerPrefs.GetInt("Runtime_Has_Nobat", 0) == 1;

        // 🌟 讀取上次裝備的技能，防止跨場景後變回 None
        currentSkill = (SkillType)PlayerPrefs.GetInt("Runtime_CurrentSkill", 0);

        UpdateSkillButtonsVisibility();
    }

    // 🌟 新增：保存當前裝備的技能
    private void SaveCurrentSkill()
    {
        PlayerPrefs.SetInt("Runtime_CurrentSkill", (int)currentSkill);
        PlayerPrefs.Save();
    }

    public void UnlockSerunai()
    {
        isSerunaiUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Serunai", 1);
        PlayerPrefs.Save(); // 🌟 強制寫入記憶體，防止跳轉場景時遺失
        UpdateSkillButtonsVisibility();
        
        if (currentSkill == SkillType.None) 
        {
            currentSkill = SkillType.Serunai;
            SaveCurrentSkill();
        }
    }

    public void UnlockGong()
    {
        isGongUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Gong", 1);
        PlayerPrefs.Save(); // 🌟 強制寫入記憶體
        UpdateSkillButtonsVisibility();
        
        if (currentSkill == SkillType.None) 
        {
            currentSkill = SkillType.Gong;
            SaveCurrentSkill();
        }
    }

    public void UnlockGendang()
    {
        isGendangUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Gendang", 1);
        PlayerPrefs.Save(); // 🌟 強制寫入記憶體
        UpdateSkillButtonsVisibility();
        
        if (currentSkill == SkillType.None) 
        {
            currentSkill = SkillType.Gendang;
            SaveCurrentSkill();
        }
    }

    public void UnlockNobat()
    {
        isNobatUnlocked = true;
        PlayerPrefs.SetInt("Runtime_Has_Nobat", 1);
        PlayerPrefs.Save(); // 🌟 強制寫入記憶體
        UpdateSkillButtonsVisibility();
        
        if (currentSkill == SkillType.None) 
        {
            currentSkill = SkillType.Nobat;
            SaveCurrentSkill();
        }
    }

    public bool HasAnySkillUnlocked()
    {
        return isSerunaiUnlocked || isGongUnlocked || isGendangUnlocked || isNobatUnlocked;
    }

    public void UpdateSkillButtonsVisibility()
    {
        if (interactButton != null) interactButton.SetActive(HasAnySkillUnlocked());

        if (btnSerunai != null) btnSerunai.SetActive(isSerunaiUnlocked);
        if (btnGong != null) btnGong.SetActive(isGongUnlocked);
        if (btnGendang != null) btnGendang.SetActive(isGendangUnlocked);
        if (btnNobat != null) btnNobat.SetActive(isNobatUnlocked);
    }

    public void ShowPanel()
    {
        if (!HasAnySkillUnlocked()) return;

        UpdateSkillButtonsVisibility(); 

        if (skillPanelUI != null) skillPanelUI.SetActive(true);
        if (NusaController.Instance != null) NusaController.Instance.DisableMovement();

        if (NusaPromptManager.Instance != null) NusaPromptManager.Instance.SuppressPrompt();
    }

    public void HidePanel()
    {
        if (skillPanelUI != null) skillPanelUI.SetActive(false);
        if (NusaController.Instance != null) NusaController.Instance.EnableMovement();
        if (NusaPromptManager.Instance != null) NusaPromptManager.Instance.RestorePrompt();
    }

    // 🌟 玩家在面板選中技能時，也要同步保存
    public void SelectSkill_Serunai() { currentSkill = SkillType.Serunai; SaveCurrentSkill(); HidePanel(); }
    public void SelectSkill_Gong()    { currentSkill = SkillType.Gong;    SaveCurrentSkill(); HidePanel(); }
    public void SkillSelect_Gendang() { currentSkill = SkillType.Gendang; SaveCurrentSkill(); HidePanel(); } 
    public void SelectSkill_Gendang() { currentSkill = SkillType.Gendang; SaveCurrentSkill(); HidePanel(); }
    public void SelectSkill_Nobat()   { currentSkill = SkillType.Nobat;   SaveCurrentSkill(); HidePanel(); }

    public void CastCurrentSkill()
    {
        if (!HasAnySkillUnlocked() || currentSkill == SkillType.None) return;

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