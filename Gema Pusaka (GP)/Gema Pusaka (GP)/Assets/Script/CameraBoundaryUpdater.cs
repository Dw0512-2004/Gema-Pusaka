using UnityEngine;
using Unity.Cinemachine; // 新版 Cinemachine 命名空间
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CinemachineConfiner2D))]
public class CameraBoundaryUpdater : MonoBehaviour
{
    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 遇到核心初始场景直接跳过，不执行寻找边界的逻辑
        if (scene.name == "Core_Settings" || scene.name == "core_settings" || scene.name == "Main_Menu") 
        {
            return;
        }

        // 延迟一帧查找，确保异步加载后的场景物体已经完全生成
        StartCoroutine(FindAndAssignBounds(scene.name));
    }

    private System.Collections.IEnumerator FindAndAssignBounds(string sceneName)
    {
        // 等待一帧
        yield return null;

        GameObject boundsObj = GameObject.Find("CameraBounds");
        
        if (boundsObj != null)
        {
            Collider2D boundsCollider = boundsObj.GetComponent<Collider2D>();
            if (boundsCollider != null && confiner != null)
            {
                confiner.BoundingShape2D = boundsCollider; 
                confiner.InvalidateBoundingShapeCache();   
            }
        }
        else
        {
            Debug.LogWarning($"当前场景 {sceneName} 没有找到 CameraBounds，相机将没有边界限制。");
        }
    }
}