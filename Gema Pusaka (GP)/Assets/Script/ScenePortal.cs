using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class ScenePortal : MonoBehaviour
{
    [Header("跨场景传送设置")]
    [Tooltip("目标场景的名称（要去的那个 Scene 名字）")]
    public string targetSceneName;

    [Tooltip("到达目标场景后，玩家会在哪个 SpawnPoint 现身（必须与目标场景中某个 PlayerSpawnPoint 的 pointID 一致）")]
    public string targetSpawnID;

    private void Awake()
    {
        // 自动确保触发器开启
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            boxCol.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 锁住玩家移动，防止穿模或在转场时乱跑
            if (NusaController.Instance != null)
            {
                NusaController.Instance.canMove = false;
                Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            // 2. 把目标出生点 ID 存入全局静态变量，供新场景的 PlayerSpawnPoint 识别
            GameManager.nextSpawnPoint = targetSpawnID;

            // 3. 呼叫全局 LoadingManager 带进度条丝滑切场景
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadScene(targetSceneName);
            }
            else
            {
                // 保底机制：若无 LoadingManager 则直接异步加载
                Debug.LogWarning("未找到 LoadingManager，将采用默认异步加载。");
                StartCoroutine(FallbackLoadRoutine());
            }

            Debug.Log($"<color=cyan>【传送门触发】准备前往场景: {targetSceneName}，对接出生点: {targetSpawnID}</color>");
        }
    }

    private IEnumerator FallbackLoadRoutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}