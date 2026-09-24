using UnityEngine;
using System.Collections.Generic;

public class MapUIManager : MonoBehaviour
{
    [System.Serializable]
    public class MapPoint
    {
        [Tooltip("必須與 MapZoneManager 中的大區域名稱完全一致")]
        public string zoneName;
        [Tooltip("地圖 UI 上對應的空節點位置")]
        public RectTransform targetPosition;
    }

    [Header("UI 引用")]
    [Tooltip("代表玩家目前位置的圖標")]
    public RectTransform playerIndicator;

    [Header("地圖座標設定")]
    public List<MapPoint> mapPoints = new List<MapPoint>();

    // 當這個 Panel 被啟動 (SetActive(true)) 時，Unity 會自動呼叫 OnEnable
    private void OnEnable()
    {
        UpdatePlayerPosition();
    }

    public void UpdatePlayerPosition()
    {
        // 確保防呆
        if (MapZoneManager.Instance == null || playerIndicator == null) return;

        // 取得目前玩家所在的大區域
        string currentZone = MapZoneManager.Instance.currentZoneName;

        // 尋找對應的座標並移動圖標
        foreach (var point in mapPoints)
        {
            if (point.zoneName == currentZone && point.targetPosition != null)
            {
                // 將圖標瞬間移動到預設的定點
                playerIndicator.position = point.targetPosition.position;
                playerIndicator.gameObject.SetActive(true);
                return;
            }
        }

        // 如果找不到匹配的區域（例如在主選單），就隱藏玩家圖標
        playerIndicator.gameObject.SetActive(false);
    }
}