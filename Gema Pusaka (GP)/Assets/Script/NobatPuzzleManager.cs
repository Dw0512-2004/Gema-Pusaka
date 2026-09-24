using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro; // 引入 TextMeshPro

public class NobatPuzzleManager : MonoBehaviour
{
    public static NobatPuzzleManager Instance;

    [Header("UI 引用")]
    public GameObject puzzlePanel;
    [Tooltip("0: Serunai, 1: Gong, 2: Gendang")]
    public Button[] instrumentButtons; 
    
    [Header("UI 提示文字")]
    public TextMeshProUGUI promptText; // 新增：用於顯示進度或錯誤訊息
    public Color normalTextColor = Color.white;
    public Color errorTextColor = Color.red;

    [Header("音频设置")]
    public AudioSource audioSource;
    public AudioClip[] instrumentSounds; 
    public AudioClip successSound; 
    public AudioClip errorSound; 

    [Header("视觉反馈颜色")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color wrongColor = Color.red;

    // 遊戲邏輯變數
    private List<int> currentSequence = new List<int>();
    private int currentRound = 0; 
    private int playerInputIndex = 0; 
    private bool isPlayerTurn = false; 
    private int[] roundDifficulties = { 3, 5, 6 }; // 難度：3音、5音、6音

    // 用於通知場景物件「解謎已成功」的回調
    private UnityAction onPuzzleSolvedCallback; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
    }

    public void StartPuzzle(UnityAction onSuccess)
    {
        onPuzzleSolvedCallback = onSuccess;
        puzzlePanel.SetActive(true);
        currentRound = 0; 
        
        // 初始化提示文字
        if (promptText != null) 
        {
            promptText.text = "0/3";
            promptText.color = normalTextColor;
        }
        
        if (NusaController.Instance != null) NusaController.Instance.DisableMovement();

        StartCoroutine(StartRoundSequence());
    }

    private IEnumerator StartRoundSequence()
    {
        isPlayerTurn = false;
        
        // 如果畫面上目前是顯示失敗訊息，在新回合開始前清除它（保留成功進度）
        if (promptText != null && promptText.text == "Please try again")
        {
            promptText.text = currentRound + "/3";
            promptText.color = normalTextColor;
        }

        yield return new WaitForSeconds(1.0f); // 準備時間

        GenerateRandomSequence();
        yield return StartCoroutine(PlaySystemSequence());
    }

    private void GenerateRandomSequence()
    {
        currentSequence.Clear();
        int notesCount = roundDifficulties[currentRound]; 
        
        for (int i = 0; i < notesCount; i++)
        {
            currentSequence.Add(Random.Range(0, 3)); 
        }
    }

    private IEnumerator PlaySystemSequence()
    {
        isPlayerTurn = false;
        
        foreach (int noteIndex in currentSequence)
        {
            // 系統自動播放時，等待音效播完
            yield return StartCoroutine(FlashButton(noteIndex, highlightColor));
            yield return new WaitForSeconds(0.2f); // 音符之間的微小間隔，讓聽覺更清晰
        }

        isPlayerTurn = true; 
        playerInputIndex = 0;
    }

    public void OnInstrumentPressed(int instrumentIndex)
    {
        // 如果還沒輪到玩家，或者上一個音效還沒播完，直接無視點擊
        if (!isPlayerTurn) return;
        StartCoroutine(HandlePlayerInput(instrumentIndex));
    }

    private IEnumerator HandlePlayerInput(int instrumentIndex)
    {
        // 【防狂按鎖定】玩家一按下按鈕，立刻鎖住輸入
        isPlayerTurn = false; 

        // 玩家按對了
        if (instrumentIndex == currentSequence[playerInputIndex])
        {
            // 等待音效與視覺閃爍完畢
            yield return StartCoroutine(FlashButton(instrumentIndex, highlightColor));
            
            playerInputIndex++;

            if (playerInputIndex >= currentSequence.Count)
            {
                if (successSound != null) audioSource.PlayOneShot(successSound);
                currentRound++;

                // 🌟 新增：顯示成功進度
                if (promptText != null)
                {
                    promptText.color = normalTextColor;
                    promptText.text = currentRound + "/3";
                }

                // 連續成功3次 (過關)
                if (currentRound >= 3)
                {
                    Debug.Log("<color=green>【Nobat 解謎】成功連續通關 3 次！</color>");
                    yield return new WaitForSeconds(1f);
                    EndPuzzle(true);
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                    StartCoroutine(StartRoundSequence());
                }
            }
            else
            {
                // 音效播完，且這回合還沒結束，解鎖讓玩家按下一顆
                isPlayerTurn = true; 
            }
        }
        // 玩家按錯了
        else
        {
            if (errorSound != null) audioSource.PlayOneShot(errorSound);
            
            // 🌟 新增：顯示失敗提示
            if (promptText != null)
            {
                promptText.color = errorTextColor;
                promptText.text = "Please try again";
            }

            foreach (Button btn in instrumentButtons)
            {
                btn.GetComponent<Image>().color = wrongColor;
            }
            
            yield return new WaitForSeconds(1f);
            
            foreach (Button btn in instrumentButtons)
            {
                btn.GetComponent<Image>().color = normalColor;
            }

            // 失敗重置進度
            currentRound = 0; 
            Debug.Log("<color=red>【Nobat 解謎】按錯了！進度歸零重新開始。</color>");
            StartCoroutine(StartRoundSequence());
        }
    }

    // 動態獲取音效長度
    private IEnumerator FlashButton(int index, Color flashColor)
    {
        Image btnImage = instrumentButtons[index].GetComponent<Image>();
        btnImage.color = flashColor;
        
        float waitTime = 1.0f; // 保底等待 1 秒
        
        // 檢查陣列範圍並確認音效是否存在
        if (instrumentSounds != null && index < instrumentSounds.Length && instrumentSounds[index] != null)
        {
            audioSource.PlayOneShot(instrumentSounds[index]);
            waitTime = instrumentSounds[index].length; // 獲取音效的精準時長
        }

        // 等待音效播放完畢
        yield return new WaitForSeconds(waitTime);
        
        btnImage.color = normalColor;
    }

    public void EndPuzzle(bool isSuccess)
    {
        puzzlePanel.SetActive(false);
        if (NusaController.Instance != null) NusaController.Instance.EnableMovement();

        if (isSuccess && onPuzzleSolvedCallback != null)
        {
            onPuzzleSolvedCallback.Invoke(); 
        }
    }
}