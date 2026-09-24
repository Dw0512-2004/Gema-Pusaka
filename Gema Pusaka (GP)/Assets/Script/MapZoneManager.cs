using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapZoneManager : MonoBehaviour
{
    public static MapZoneManager Instance;

    [System.Serializable]
    public class ZoneData
    {
        [Tooltip("大區域的名稱，例如：Main Entrance")]
        public string zoneName;
        
        [Tooltip("屬於這個大區域的所有實際 Scene 名稱")]
        public List<string> sceneNames;
    }

    [Header("地圖區域劃分設定")]
    public List<ZoneData> zoneDatabase = new List<ZoneData>();

    [Header("當前狀態 (唯讀觀察用)")]
    public string currentSceneName;
    public string currentZoneName;

    private void Awake()
    {
        // 因為掛在 Nusa 身上，生命週期由 NusaController 掌控
        // 這裡只需要確保單例模式不衝突，且「只銷毀多餘的腳本組件 (this)」，不要銷毀整個 GameObject
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this); 
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 只要 Nusa 活著載入了新場景，這裡就會自動更新區域
        currentSceneName = scene.name;
        currentZoneName = GetZoneBySceneName(scene.name);
        
        Debug.Log($"<color=orange>【地圖更新】載入場景: {currentSceneName} -> 歸屬於大區域: {currentZoneName}</color>");
    }

    /// <summary>
    /// 透過場景名稱反查所屬的大區域名稱
    /// </summary>
    public string GetZoneBySceneName(string targetScene)
    {
        foreach (var zone in zoneDatabase)
        {
            if (zone.sceneNames.Contains(targetScene))
            {
                return zone.zoneName;
            }
        }
        
        // 如果場景沒有被登記，直接回傳原名防呆
        return targetScene; 
    }
}