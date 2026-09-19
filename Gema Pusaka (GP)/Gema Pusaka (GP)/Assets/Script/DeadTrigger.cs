using UnityEngine;

public class DeadTrigger : MonoBehaviour
{
    [Tooltip("勾选后，这个陷阱可以一击必杀并重生玩家")]
    public bool isInstantKill = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 判断碰到的物体是不是玩家 (可以通过 Tag 判定，或者直接找 NusaController 组件)
        if (collision.CompareTag("Player"))
        {
            if (NusaController.Instance != null && isInstantKill)
            {
                // 呼叫重生方法
                NusaController.Instance.RespawnAtLastSafeGround();
            }
        }
    }
}