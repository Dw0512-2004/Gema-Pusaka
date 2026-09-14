using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransition : MonoBehaviour
{
    [Tooltip("目标场景的名称")]
    public string sceneToLoad;
    [Tooltip("进入新场景后玩家出生点的标签")]
    public string exitMark;

    private void Awake()
    {
        // 确保触发器的 BoxCollider2D 自动开启 Is Trigger
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
            // 锁定玩家移动，防止穿模
            if (NusaController.Instance != null)
            {
                NusaController.Instance.canMove = false;
                Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            // 存入全局变量，供下一个场景的 PlayerSpawnPoint 识别
            GameManager.nextSpawnPoint = exitMark;

            // 【核心修改】：通过全局 LoadingManager 触发平滑过渡与进度条加载
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadScene(sceneToLoad);
            }
            else
            {
                // 保底：若无 LoadingManager 实例，则用原生物理异步加载
                Debug.LogWarning("未找到 LoadingManager，将采用默认异步加载。");
                StartCoroutine(TransitionRoutineFallback());
            }
        }
    }

    private IEnumerator TransitionRoutineFallback()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}