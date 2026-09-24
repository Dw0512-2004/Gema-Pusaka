using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NobatInteractableTarget : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("玩家必須距離這個物體多近，使用 Nobat 才能觸發解謎？")]
    public float interactRadius = 3f;
    
    [Tooltip("解謎成功後要執行的事件（可以拖入 Fungus 的 Flowchart -> Execute Block）")]
    public UnityEvent OnPuzzleSolved;

    private bool isAlreadySolved = false;
    private bool isWaitingForAudio = false; // 新增：防止等待音效時重複觸發

    // --- 新增：組件引用 ---
    private Collider2D obstacleCollider;
    private SpriteRenderer obstacleSprite;

    private void Awake()
    {
        // 遊戲開始時自動抓取掛在同一個物件上的 Collider 和 SpriteRenderer
        obstacleCollider = GetComponent<Collider2D>();
        obstacleSprite = GetComponent<SpriteRenderer>();
    }

    // 訂閱與取消訂閱全域技能事件 (最佳實踐，防止記憶體洩漏)
    private void OnEnable()
    {
        SkillManager.OnNobatUsed += CheckNobatActivation;
    }

    private void OnDisable()
    {
        SkillManager.OnNobatUsed -= CheckNobatActivation;
    }

    // 當玩家按下 Nobat 時，這個方法會被自動喚醒
    private void CheckNobatActivation(float soundDuration)
    {
        if (isAlreadySolved || isWaitingForAudio) return; // 已經解鎖或正在等待時忽略
        if (NusaController.Instance == null) return;

        // 計算玩家與目標的距離
        float distance = Vector2.Distance(transform.position, NusaController.Instance.transform.position);
        
        if (distance <= interactRadius)
        {
            Debug.Log($"<color=cyan>【Nobat 目標】偵測到 Nobat，距離足夠，等待 {soundDuration} 秒後啟動面板！</color>");
            
            // 啟動協程，等待音效播完才開面板
            StartCoroutine(WaitAudioAndStartPuzzle(soundDuration));
        }
        else
        {
            Debug.Log($"<color=grey>【Nobat 目標】聽到 Nobat，但距離太遠 ({distance} > {interactRadius})</color>");
        }
    }

    // 新增：等待音效長度的協程
    private IEnumerator WaitAudioAndStartPuzzle(float delay)
    {
        isWaitingForAudio = true;

        // 1. 在等待音效期間先鎖住玩家移動，避免玩家邊吹邊跑
        if (NusaController.Instance != null)
        {
            NusaController.Instance.DisableMovement();
        }

        // 2. 完美等待技能音效播完
        yield return new WaitForSeconds(delay);

        isWaitingForAudio = false;

        // 3. 音效結束，正式呼叫 UI 開啟解謎，並傳入成功後的回調
        if (NobatPuzzleManager.Instance != null)
        {
            NobatPuzzleManager.Instance.StartPuzzle(PuzzleCompleted);
        }
        else
        {
            // 保底：如果場景裡沒放面板，恢復玩家控制
            if (NusaController.Instance != null) NusaController.Instance.EnableMovement();
        }
    }

    // 解謎成功後由 UI 回調觸發
    private void PuzzleCompleted()
    {
        isAlreadySolved = true;
        Debug.Log("<color=yellow>【Nobat 目標】封印解除 / 記憶恢復！</color>");
        
        // ==========================================
        // 🌟 新增：解謎成功後關閉碰撞體和圖片
        // ==========================================
        if (obstacleCollider != null) obstacleCollider.enabled = false;
        if (obstacleSprite != null) obstacleSprite.enabled = false;

        // 觸發你在 Inspector 裡設定的 UnityEvent (例如播放門打開的動畫，或呼叫 Fungus 對話)
        OnPuzzleSolved?.Invoke();
    }

    // 在編輯器畫出檢測範圍的圓圈，方便你調整距離
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}