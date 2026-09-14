using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject); // 只保留 Loading UI 跨场景存活
            Debug.Log("<color=green>【CoreManager】加载面板核心已激活并設為 DontDestroyOnLoad。</color>");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}