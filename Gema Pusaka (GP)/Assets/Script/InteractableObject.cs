using UnityEngine;
using UnityEngine.Events;
using Fungus; // 引入 Fungus 命名空间

[RequireComponent(typeof(BoxCollider2D))]
public class InteractableObject : MonoBehaviour
{
    [Header("UI 提示設定")]
    [Tooltip("走近這個物件時，Nusa 頭頂要顯示的自定義提示詞")]
    [TextArea(1, 3)]
    public string promptMessage = "按 E 互動";

    [Tooltip("走近時要顯示的小圖標/Icon（可選，不放則只顯示純文字）")]
    public Sprite promptIcon;

    [Header("Fungus 對話設定 (可選)")]
    [Tooltip("如果這個物件是要對話的 NPC，直接在這裡指定 Flowchart")]
    public Flowchart targetFlowchart;
    [Tooltip("你想從這個 Flowchart 的哪個 Block 開始播放對話")]
    public string targetBlockName = "Start";

    [Header("其他自定義事件 (可選)")]
    [Tooltip("除了 Fungus 之外，還想額外觸發的事件（如開啟寶箱、播放動畫等）")]
    public UnityEvent onInteracted;

    private bool playerInRange = false;
    private bool hasInteracted = false;
    private Collider2D objectCollider; // 緩存 Collider 變數

    private void Start()
    {
        // 確保 Collider 是 Trigger 模式，並將其緩存起來
        objectCollider = GetComponent<Collider2D>();
        if (objectCollider != null && !objectCollider.isTrigger)
        {
            objectCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            hasInteracted = false;

            // 顯示 Nusa 頭頂的提示詞與 Icon
            if (NusaPromptManager.Instance != null)
            {
                NusaPromptManager.Instance.ShowPrompt(promptMessage, promptIcon);
            }
            Debug.Log($"<color=cyan>【互動範圍】玩家已走進 {gameObject.name} 的範圍。</color>");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            hasInteracted = false; // 離開範圍時重置互動鎖定

            // 隱藏 Nusa 頭頂的提示
            if (NusaPromptManager.Instance != null)
            {
                NusaPromptManager.Instance.HidePrompt();
            }
            Debug.Log($"<color=cyan>【互動範圍】玩家已離開 {gameObject.name} 的範圍。</color>");
        }
    }

    private void OnMouseDown()
    {
        // 支援滑鼠點擊互動
        if (playerInRange && !hasInteracted)
        {
            TryInteract();
        }
    }

   private void Update()
    {
        // 確保玩家在範圍內且尚未互動
        if (playerInRange && !hasInteracted)
        {
            // 1. 支援鍵盤按鍵互動 (E 鍵或 Enter)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                TryInteract();
            }

            // 2. 支援滑鼠左鍵 / 手機螢幕點擊 (純數學判定，無視任何 UI 遮擋)
            if (Input.GetMouseButtonDown(0))
            {
                // 將滑鼠或手指在螢幕上的位置，轉換為 2D 世界座標
                Vector2 tapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // 檢查點擊的位置是否精準落在此物件的 Collider 範圍內
                if (objectCollider != null && objectCollider.OverlapPoint(tapPosition))
                {
                    TryInteract();
                }
            }
        }
    }

    public void TryInteract()
    {
        if (!playerInRange || hasInteracted) return;

        // 【關鍵修復】：不要檢查整個 Flowchart，只檢查我們要呼叫的那個 Block 是否正在播放
        if (targetFlowchart != null && !string.IsNullOrEmpty(targetBlockName))
        {
            Block targetBlock = targetFlowchart.FindBlock(targetBlockName);
            if (targetBlock != null && targetBlock.IsExecuting())
            {
                return; // 如果這段專屬對話正在播，那就退出，不要重複觸發
            }
        }

        hasInteracted = true;

        // 1. 互動瞬間立刻隱藏 Nusa 的提示
        if (NusaPromptManager.Instance != null)
        {
            NusaPromptManager.Instance.HidePrompt();
        }

        Debug.Log($"<color=green>【互動成功】觸發物件：{gameObject.name}</color>");

        // 2. 如果是 Encounter (遭遇戰)，觸發後直接關閉 Collider
        if (gameObject.CompareTag("Encounter"))
        {
            if (objectCollider != null)
            {
                objectCollider.enabled = false;
                Debug.Log($"<color=yellow>【狀態更新】已關閉 {gameObject.name} 的 Collider，這是一次性遭遇事件。</color>");
            }
        }

        // 3. 執行 Fungus 對話
        if (targetFlowchart != null && !string.IsNullOrEmpty(targetBlockName))
        {
            Debug.Log($"<color=green>【Fungus】成功觸發對話塊: {targetBlockName}</color>");
            targetFlowchart.ExecuteBlock(targetBlockName);
        }

        // 4. 同時觸發 Inspector 裡綁定的其他自定義事件（UnityEvent）
        onInteracted?.Invoke();
    }
}