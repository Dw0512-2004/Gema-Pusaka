using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Tooltip("这个出生点的专属ID，需与传送门填写的 Target Spawn ID 一致")]
    public string pointID;

    private void Start()
    {
        // 1. 印出驗票過程，幫我們確認車票有沒有成功帶過來
        Debug.Log($"<color=yellow>【出生點驗票】我是 '{pointID}'，目前全域車票寫著: '{GameInitializer.nextSpawnPoint}'</color>");

        if (GameInitializer.nextSpawnPoint == pointID)
        {
            if (NusaController.Instance != null)
            {
                // 瞬间将玩家移动到当前位置，并保持 Z 轴不变
                Vector3 targetPos = transform.position;
                targetPos.z = NusaController.Instance.transform.position.z;
                NusaController.Instance.transform.position = targetPos;

                // 清空物理惯性
                Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }

                // 传送完成，恢复玩家移动权限与时间
                NusaController.Instance.EnableMovement();
                Time.timeScale = 1f;
                
                // 清空记录，防止重复触发
                GameInitializer.nextSpawnPoint = "";

                Debug.Log($"<color=green>【传送成功】玩家已精准对齐出生点：{pointID}</color>");
            }
            else
            {
                Debug.LogError("<color=red>【传送失败】暗號對上了，但找不到 NusaController！</color>");
            }
        }
    }
}