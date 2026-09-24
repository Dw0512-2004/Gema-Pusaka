using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Fungus; // 引入 Fungus 命名空間

[RequireComponent(typeof(AudioSource))]
public class PuzzleSealController : MonoBehaviour
{
    [Header("謎題 UI 設定")]
    [Tooltip("請把內圈、中圈、外圈的 UI Image (RectTransform) 拖進來")]
    public RectTransform[] rings;
    
    [Tooltip("圓環旋轉的速度")]
    public float rotateSpeed = 5f;

    [Header("音效設定")]
    [Tooltip("解謎成功時播放的音效")]
    public AudioClip successSound;
    private AudioSource audioSource;

    [Header("Fungus 聯動設定")]
    [Tooltip("用來控制結局對話的 Flowchart")]
    public Flowchart targetFlowchart;
    [Tooltip("解謎成功後，要觸發的 Block 名稱 (例如：Puzzle1_Win)")]
    public string successBlockName = "Puzzle1_Win";

    [Header("額外通關事件")]
    [Tooltip("解開謎題時，你可以讓面板自動關閉、播放成功音效等")]
    public UnityEvent onPuzzleSolved;

    private int[] targetAngles;
    private bool[] isRotating;
    private bool isSolved = false;

    private void Awake()
    {
        // 自動抓取掛在同一個物件上的 AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        InitializePuzzle();
    }

    // 每次打開謎題時，隨機打亂圓盤
    public void InitializePuzzle()
    {
        isSolved = false;
        targetAngles = new int[rings.Length];
        isRotating = new bool[rings.Length];

        for (int i = 0; i < rings.Length; i++)
        {
            // 隨機轉動 1~3 次 (也就是 90, 180, 270 度)，確保初始狀態一定是錯亂的
            int randomTurns = Random.Range(1, 4);
            targetAngles[i] = randomTurns * 90;
            rings[i].localEulerAngles = new Vector3(0, 0, targetAngles[i]);
            isRotating[i] = false;
        }
    }

    // 提供給 UI Button 點擊的事件，傳入 0 代表內圈，1 代表中圈，以此類推
    public void OnRingClicked(int ringIndex)
    {
        // 如果已經通關，或者該圓環正在旋轉中，則不作反應
        if (isSolved || isRotating[ringIndex]) return;

        StartCoroutine(RotateRingSmoothly(ringIndex));
    }

    // 處理平滑旋轉的協程
    private IEnumerator RotateRingSmoothly(int index)
    {
        isRotating[index] = true;
        
        float startAngle = targetAngles[index];
        targetAngles[index] += 90; // 每次點擊順時針旋轉 90 度

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * rotateSpeed;
            // 透過 Lerp 讓旋轉看起來有平滑過渡的動畫感
            float currentZ = Mathf.Lerp(startAngle, targetAngles[index], t);
            rings[index].localEulerAngles = new Vector3(0, 0, currentZ);
            yield return null;
        }

        // 確保精準對齊，並取餘數讓數值保持在 0~360 之間
        rings[index].localEulerAngles = new Vector3(0, 0, targetAngles[index]);
        targetAngles[index] = targetAngles[index] % 360;
        isRotating[index] = false;

        CheckWinCondition();
    }

    // 檢查是否所有圓盤都歸零
    private void CheckWinCondition()
    {
        foreach (int angle in targetAngles)
        {
            // 如果有任何一個圓環的角度不是 0，就代表還沒過關
            if (angle != 0) return; 
        }

        isSolved = true;
        Debug.Log("<color=green>【解謎成功】歷史的封印已解開！準備播放音效與廣播 Fungus 事件...</color>");

        // 🌟 播放專屬的成功音效
        if (successSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        // 1. 發送 Fungus 全局事件 (Event Message)
        if (!string.IsNullOrEmpty(successBlockName))
        {
            Debug.Log($"<color=cyan>【Fungus】發送 Event Message: {successBlockName}</color>");
            Fungus.Flowchart.BroadcastFungusMessage(successBlockName);
        }
        else
        {
            Debug.LogError("【錯誤】Success Block Name 是空的，無法發送廣播！");
        }

        // 2. 觸發自定義事件 (隱藏解謎面板)
        onPuzzleSolved?.Invoke();
    }
}