using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class PuzzleResonanceController : MonoBehaviour
{
    [Header("UI 滑桿引用")]
    [Tooltip("代表 Serunai 氣息的滑桿")]
    public Slider breathSlider;
    [Tooltip("代表 Gong 重量的滑桿")]
    public Slider weightSlider;
    [Tooltip("代表 Gendang 節奏的滑桿")]
    public Slider tempoSlider;

    [Header("指標與進度 UI")]
    [Tooltip("在軌道上移動的指標 (RectTransform)")]
    public RectTransform indicator;
    [Tooltip("用來顯示 3 秒倒數的進度條 (Image 的 Image Type 設為 Filled)")]
    public Image progressBar;

    [Header("平衡參數設定")]
    [Tooltip("指標移動範圍的最小與最大 X 座標")]
    public float minX = -300f;
    public float maxX = 300f;
    
    [Tooltip("目標共鳴區(綠區)的範圍")]
    public float targetMinX = -30f;
    public float targetMaxX = 30f;

    [Tooltip("需要維持在共鳴區內的時間 (秒)")]
    public float requiredHoldTime = 3f;

    [Header("音效設定")]
    [Tooltip("解謎成功時播放的音效")]
    public AudioClip successSound;
    private AudioSource audioSource;

    [Header("Fungus 聯動設定")]
    [Tooltip("解謎成功後要廣播的 Fungus Message")]
    public string successMessage = "Puzzle2_Win";
    public UnityEvent onPuzzleSolved;

    private float holdTimer = 0f;
    private bool isSolved = false;

    private void Awake()
    {
        // 自動抓取掛在同一個物件上的 AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isSolved) return;

        // 1. 取得滑桿數值
        float breath = breathSlider.value;
        float weight = weightSlider.value;
        float tempo = tempoSlider.value;

        // 2. 計算指針目標位置 
        float balanceFactor = (breath * 1.5f) - (weight * 1.5f) + (tempo * 0.5f) - 0.25f;
        float targetX = Mathf.Clamp(balanceFactor * 200f, minX, maxX);

        // 3. 讓指針平滑移動到目標位置
        Vector2 currentPos = indicator.anchoredPosition;
        indicator.anchoredPosition = Vector2.Lerp(currentPos, new Vector2(targetX, currentPos.y), Time.deltaTime * 5f);

        // 4. 判斷是否進入共鳴區 (綠區)
        float indicatorX = indicator.anchoredPosition.x;
        if (indicatorX >= targetMinX && indicatorX <= targetMaxX)
        {
            holdTimer += Time.deltaTime;
            
            if (holdTimer >= requiredHoldTime)
            {
                WinPuzzle();
            }
        }
        else
        {
            holdTimer = Mathf.Max(0, holdTimer - Time.deltaTime * 1.5f);
        }

        // 5. 更新進度條 UI
        if (progressBar != null)
        {
            progressBar.fillAmount = holdTimer / requiredHoldTime;
        }
    }

    private void WinPuzzle()
    {
        isSolved = true;
        Debug.Log("<color=green>【解謎成功】靈魂共鳴達成！</color>");

        // 播放專屬的成功音效
        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        // 廣播 Fungus 事件
        if (!string.IsNullOrEmpty(successMessage))
        {
            Fungus.Flowchart.BroadcastFungusMessage(successMessage);
        }

        // 觸發自定義事件 (例如關閉面板)
        onPuzzleSolved?.Invoke();
    }

    // 當面板開啟時重置狀態
    private void OnEnable()
    {
        isSolved = false;
        holdTimer = 0f;
        if (progressBar != null) progressBar.fillAmount = 0f;
        
        if (breathSlider) breathSlider.value = 0.9f; 
        if (weightSlider) weightSlider.value = 0.1f; 
        if (tempoSlider) tempoSlider.value = 0.2f;    
    }
}