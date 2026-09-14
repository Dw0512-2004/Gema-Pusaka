using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Tooltip("这个出生点的专属ID，需与传送门填写的 exitMark 一致")]
    public string pointID;

    private void Start()
    {
        // 当新场景加载完成，所有出生点苏醒。
        // 核对全局变量，如果是找自己的，就把玩家拉过来。
        if (GameManager.nextSpawnPoint == pointID)
        {
            if (NusaController.Instance != null)
            {
                // 瞬间将玩家移动到当前位置，并保持 Z 轴不变
                Vector3 targetPos = transform.position;
                targetPos.z = NusaController.Instance.transform.position.z;
                NusaController.Instance.transform.position = targetPos;

                // 【优化点】：清空物理惯性，防止复活或传送时带着掉落惯性
                Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }

                // 传送完成，恢复玩家移动权限与时间
                NusaController.Instance.canMove = true;
                Time.timeScale = 1f;
                
                // 清空记录，防止重复触发
                GameManager.nextSpawnPoint = "";

                Debug.Log($"<color=green>【传送成功】玩家已精准对齐出生点：{pointID}</color>");
            }
        }
    }
}