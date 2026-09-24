using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class PuzzleRhythmController : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("來回移動的游標 (RectTransform)")]
    public RectTransform indicator;
    [Tooltip("目標區域 (Sweet Spot) 的 RectTransform")]
    public RectTransform sweetSpot;
    [Tooltip("顯示目前成功次數的 UI 文字 (可選)")]
    public Text progressText;

    [Header("遊戲參數設定")]
    [Tooltip("游標移動的速度")]
    public float moveSpeed = 500f;
    [Tooltip("軌道左右移動的邊界範圍 (-X 到 +X)")]
    public float trackWidth = 250f;
    [Tooltip("通關需要的成功點擊次數")]
    public int requiredHits = 3;

    [Header("音效設定")]
    [Tooltip("解謎成功時播放的音效")]
    public AudioClip successSound;
    private AudioSource audioSource;

    [Header("Fungus 聯動設定")]
    [Tooltip("解謎成功後要廣播的 Fungus Message")]
    public string successMessage = "Puzzle3_Win";
    public UnityEvent onPuzzleSolved;

    private int currentHits = 0;
    private bool isSolved = false;
    private int direction = 1; // 1 代表向右，-1 代表向左

    private void Awake()
    {
        // 自動抓取掛在同一個物件上的 AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isSolved) return;

        // 1. 讓游標在軌道上左右來回移動
        float newX = indicator.anchoredPosition.x + (direction * moveSpeed * Time.deltaTime);
        
        // 碰到邊界時折返
        if (newX > trackWidth)
        {
            newX = trackWidth;
            direction = -1;
        }
        else if (newX < -trackWidth)
        {
            newX = -trackWidth;
            direction = 1;
        }

        indicator.anchoredPosition = new Vector2(newX, indicator.anchoredPosition.y);

        // 2. 檢測空白鍵或 Enter 鍵
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            CheckHitTiming();
        }
    }

    // 檢查點擊時機是否在目標區內
    public void OnStrikeButtonClicked()
    {
        CheckHitTiming();
    }

    private void CheckHitTiming()
    {
        if (isSolved) return;

        float indicatorX = indicator.anchoredPosition.x;
        float sweetSpotMin = sweetSpot.anchoredPosition.x - (sweetSpot.rect.width / 2f);
        float sweetSpotMax = sweetSpot.anchoredPosition.x + (sweetSpot.rect.width / 2f);

        // 判斷游標 X 座標是否落在目標區間內
        if (indicatorX >= sweetSpotMin && indicatorX <= sweetSpotMax)
        {
            currentHits++;
            Debug.Log($"<color=green>【節奏命中】成功！累積命中: {currentHits}/{requiredHits}</color>");

            // 稍微加快一點速度增加挑戰性
            moveSpeed += 50f;

            UpdateProgressText();

            if (currentHits >= requiredHits)
            {
                WinPuzzle();
            }
        }
        else
        {
            Debug.Log("<color=red>【節奏失誤】沒抓準時機！</color>");
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = $"Hits: {currentHits} / {requiredHits}";
        }
    }

    private void WinPuzzle()
    {
        isSolved = true;
        Debug.Log("<color=green>【解謎成功】自由的脈搏考驗通過！</color>");

        // 🌟 播放專屬的成功音效
        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        // 廣播 Fungus 事件
        if (!string.IsNullOrEmpty(successMessage))
        {
            Fungus.Flowchart.BroadcastFungusMessage(successMessage);
        }

        onPuzzleSolved?.Invoke();
    }

    private void OnEnable()
    {
        isSolved = false;
        currentHits = 0;
        moveSpeed = 500f;
        direction = 1;
        UpdateProgressText();
    }
}