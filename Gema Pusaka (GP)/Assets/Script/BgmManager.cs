using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance;

    [Header("背景音樂播放器")]
    [Tooltip("用於播放背景音樂 (腳本會自動將其設為 Loop)")]
    public AudioSource bgmSource;

    [Header("場景 BGM (BGM Clips)")]
    public AudioClip menuBgm;         // 適用: Main Menu, Load Data 
    public AudioClip cutsceneBgm;     // 適用: Introduction Scene, End Scene
    public AudioClip mainEntranceBgm; // 適用大區域: Main Entrance
    public AudioClip cellarBgm;       // 適用大區域: Underground Cellar
    public AudioClip ceremonyHallBgm; // 適用大區域: Ceremony Hall
    public AudioClip royalGalleryBgm; // 適用大區域: Royal Gallery
    public AudioClip libraryBgm;      // 適用大區域: Forgotten Library
    public AudioClip throneRoomBgm;   // 適用大區域: Throne Room

    private void Awake()
    {
        // 單例模式：確保整個遊戲只有一個 BgmManager 存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨場景不銷毀
            
            // 自動抓取掛在同一個物件上的 AudioSource
            bgmSource = GetComponent<AudioSource>();
            bgmSource.loop = true; // 確保背景音樂無限循環
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ==========================================
    // BGM 場景與大區域自動切換邏輯
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip nextBgm = null;

        // 預設使用場景原名（為了相容 Main Menu 等沒有 MapZoneManager 的場景）
        string currentZoneOrScene = scene.name;

        // 如果 MapZoneManager 存在，就把場景名稱轉換成大區域名稱
        if (MapZoneManager.Instance != null)
        {
            currentZoneOrScene = MapZoneManager.Instance.GetZoneBySceneName(scene.name);
        }

        // 根據大區域名稱或場景名稱，決定要播放哪一首 BGM
        switch (currentZoneOrScene)
        {
            // 非 Gameplay 場景 (依賴原場景名)
            case "Main Menu":
            case "Load Data": // 請注意：您原來的腳本中寫的是 "Load Data " (有空格)，建議統一為沒有多餘空格的名稱
                nextBgm = menuBgm;
                break;
            case "Introduction Scene":
            case "End Scene":
                nextBgm = cutsceneBgm;
                break;
            
            // Gameplay 大區域 (依賴 MapZoneManager 的 ZoneName)
            case "Main Entrance":
                nextBgm = mainEntranceBgm;
                break;
            case "Underground Cellar":
                nextBgm = cellarBgm;
                break;
            case "Ceremony Hall":
                nextBgm = ceremonyHallBgm;
                break;
            case "Royal Gallery":
                nextBgm = royalGalleryBgm;
                break;
            case "Forgotten Library":
                nextBgm = libraryBgm;
                break;
            case "Throne Room":
                nextBgm = throneRoomBgm;
                break;
        }

        // 核心邏輯：如果新場景有指定的 BGM，且跟現在正在播的不一樣，才進行切換
        if (nextBgm != null && bgmSource.clip != nextBgm)
        {
            bgmSource.clip = nextBgm;
            bgmSource.Play();
        }
    }
}