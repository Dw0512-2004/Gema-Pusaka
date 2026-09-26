using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class NusaController : MonoBehaviour
{
    // --- 单例模式 ---
    public static NusaController Instance;

    [Header("Movement Settings (移动设置)")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float maxJumpForce = 15f;
    [Tooltip("提早松开跳跃键时的向上的速度衰减倍率")]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Ground Check Settings (地面检测)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Respawn Settings (重生设置)")]
    public Vector3 lastSafePosition;
    public float timeToRecordSafePos = 0.2f;
    private float groundedTimer = 0f;
    
    [Header("Interaction Settings")]
    [SerializeField] private float holdDurationRequired = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip footstepSound; 
    [SerializeField] private AudioClip jumpSound;     
    
    // 🌟 音效播放器分離
    private AudioSource mainAudioSource;
    private AudioSource footstepSource; 

    [Header("UI References")]
    public GameObject playerUIRoot;
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject mapPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "Main_Menu";
    public string loadDataSceneName = "Load_Data_Scene";
    public string introductionSceneName = "Introduction_Scene"; 
    public string endSceneName = "End_Scene"; 

    [HideInInspector] public bool isPaused = false;
    public bool isGrounded;
    public bool isFacingRight = true;
    public bool canMove = true; 

    private bool isMovingLeft;
    private bool isMovingRight;
    private float horizontalInput;
    private bool isInteractPressed = false;
    private float holdTimer = 0f;
    private bool panelToggled = false; 

    private Rigidbody2D rb;
    private Animator an;
    private Vector2 moveVelocity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return; 
        }

        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();
        
        mainAudioSource = GetComponent<AudioSource>(); 
        
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.clip = footstepSound;
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;

        CloseAllPanels();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == endSceneName || 
            scene.name == mainMenuSceneName || 
            scene.name == loadDataSceneName || 
            scene.name == introductionSceneName)
        {
            Time.timeScale = 1f; 
            if (Instance == this) Instance = null;
            
            Destroy(gameObject, 0.1f);
            return; 
        }

        int intent = PlayerPrefs.GetInt("IsNewGame_Intent", 0);
        if (intent == 0)
        {
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
            if (PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1)
            {
                float savedX = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosX", transform.position.x);
                float savedY = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosY", transform.position.y);
                transform.position = new Vector3(savedX, savedY, transform.position.z);
                lastSafePosition = transform.position;

                PlayerPrefs.SetInt("Slot_" + activeSlot + "_HasSavedPos", 0);
                PlayerPrefs.Save();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (settingsPanel != null && settingsPanel.activeSelf) OpenPauseMenuOnly();
                else if (mapPanel != null && mapPanel.activeSelf) CloseMap();
                else ResumeGame();
            }
            else PauseGame();
        }

        if (isPaused) return; 

        Collider2D hitCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = hitCollider != null;
        
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            if (hitCollider.gameObject.layer != LayerMask.NameToLayer("HiddenPlatform"))
            {
                groundedTimer += Time.deltaTime;
                if (groundedTimer >= timeToRecordSafePos) lastSafePosition = transform.position;
            }
            else groundedTimer = 0f;
        }
        else groundedTimer = 0f; 

        if (an != null) an.SetBool("isGrounded", isGrounded);

        if (isInteractPressed && !panelToggled)
        {
            if (SkillManager.Instance != null && SkillManager.Instance.HasAnySkillUnlocked())
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdDurationRequired)
                {
                    SkillManager.Instance.ShowPanel();
                    panelToggled = true; 
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E)) SkillPointerDown();
        if (Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.E)) SkillPointerUp();

        if (!canMove)
        {
            horizontalInput = 0f;
            if (an != null) an.SetFloat("Speed", 0f);
            if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        bool leftInput = isMovingLeft || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightInput = isMovingRight || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (leftInput && !rightInput) horizontalInput = -1f;
        else if (rightInput && !leftInput) horizontalInput = 1f;
        else horizontalInput = 0f;

        if (an != null) an.SetFloat("Speed", Mathf.Abs(horizontalInput));

        bool isWalking = isGrounded && Mathf.Abs(horizontalInput) > 0.01f && canMove;
        if (footstepSource != null && footstepSound != null)
        {
            if (isWalking)
            {
                if (!footstepSource.isPlaying) footstepSource.Play();
            }
            else
            {
                if (footstepSource.isPlaying) footstepSource.Stop();
            }
        }

        if (horizontalInput > 0 && !isFacingRight) Flip();
        else if (horizontalInput < 0 && isFacingRight) Flip();

        if (Input.GetKeyDown(KeyCode.Space)) Jump();
        if (Input.GetKeyUp(KeyCode.Space)) StopJump();
    }

    private void FixedUpdate()
    {
        if (!canMove || isPaused)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        moveVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = moveVelocity;
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        DisableMovement();
        OpenPauseMenuOnly();
        if (mainAudioSource != null && mainAudioSource.isPlaying) mainAudioSource.Pause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        EnableMovement();
        CloseAllPanels();
        if (mainAudioSource != null) mainAudioSource.UnPause();
    }

    public void OpenSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OpenMap()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        DisableMovement();
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(true);
        if (mainAudioSource != null && mainAudioSource.isPlaying) mainAudioSource.Pause();
    }

    public void CloseMap() => ResumeGame();

    public void SaveGameDataToCurrentSlot()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("Slot_" + activeSlot + "_Scene", currentScene);

        int s0 = (SkillManager.Instance != null && SkillManager.Instance.isSerunaiUnlocked) ? 1 : 0;
        int s1 = (SkillManager.Instance != null && SkillManager.Instance.isGongUnlocked) ? 1 : 0;
        int s2 = (SkillManager.Instance != null && SkillManager.Instance.isGendangUnlocked) ? 1 : 0;
        int s3 = (SkillManager.Instance != null && SkillManager.Instance.isNobatUnlocked) ? 1 : 0;

        // 🌟 核心修改：以解鎖的技能數量直接計算遊戲總進度 (每個技能 25%)
        int currentProgress = (s0 * 25) + (s1 * 25) + (s2 * 25) + (s3 * 25);
        
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Progress", currentProgress);
        PlayerPrefs.SetInt("Runtime_GameProgress", currentProgress);

        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_0", s0);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_1", s1);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_2", s2);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_3", s3);

        PlayerPrefs.SetInt("Runtime_Has_Serunai", s0);
        PlayerPrefs.SetInt("Runtime_Has_Gong", s1);
        PlayerPrefs.SetInt("Runtime_Has_Gendang", s2);
        PlayerPrefs.SetInt("Runtime_Has_Nobat", s3);

        PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosX", transform.position.x);
        PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosY", transform.position.y);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_HasSavedPos", 1);

        PlayerPrefs.Save();
        Debug.Log($"<color=green>【存檔成功】槽位 {activeSlot} 技能狀態已保存！目前進度：{currentProgress}%</color>");
        
        ResumeGame();

        if (NusaPromptManager.Instance != null)
        {
            NusaPromptManager.Instance.ShowPrompt("Saved");
            Invoke(nameof(ClearSavePrompt), 2f); 
        }
    }

    private void ClearSavePrompt()
    {
        if (NusaPromptManager.Instance != null)
        {
            NusaPromptManager.Instance.HidePrompt();
        }
    }

    public void GoToLoadDataScene()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 0);
        PlayerPrefs.Save();
        SafeExitToScene(loadDataSceneName);
    }

    public void ExitToMainMenu() => SafeExitToScene(mainMenuSceneName);

    public void OpenPauseMenuOnly()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    private void CloseAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    private void SafeExitToScene(string targetSceneName)
    {
        System.Action cleanUpLogic = () => 
        {
            Time.timeScale = 1f; 
            isPaused = false;
            CloseAllPanels();
        };

        if (LoadingManager.Instance != null) LoadingManager.Instance.LoadSceneWithCleanUp(targetSceneName, cleanUpLogic);
        else
        {
            cleanUpLogic.Invoke();
            SceneManager.LoadScene(targetSceneName);
        }
    }

    public void Jump()
    {
        if (isGrounded && canMove && !isPaused)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpForce);
            if (an != null) an.SetTrigger("Jump");
            
            if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
            PlayJumpSound();
        }
    }

    public void StopJump()
    {
        if (rb.linearVelocity.y > 0) rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    public void StartMovingLeft() => isMovingLeft = true;
    public void StopMovingLeft() => isMovingLeft = false;
    public void StartMovingRight() => isMovingRight = true;
    public void StopMovingRight() => isMovingRight = false;

    public void DisableMovement()
    {
        canMove = false;
        isMovingLeft = false;
        isMovingRight = false;
        horizontalInput = 0f;
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (an != null) an.SetFloat("Speed", 0f);
        if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
    }

    public void EnableMovement() => canMove = true;

    public void SkillPointerDown()
    {
        if (isPaused) return;
        isInteractPressed = true;
        holdTimer = 0f;
        panelToggled = false;
    }

    public void SkillPointerUp()
    {
        if (isPaused) return;
        isInteractPressed = false;
        if (!panelToggled && holdTimer < holdDurationRequired && SkillManager.Instance != null) SkillManager.Instance.CastCurrentSkill(); 
        holdTimer = 0f;
    }

    public void RespawnAtLastSafeGround()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = lastSafePosition;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void PlayFootstepSound() { /* 已改由 Update 控制 */ }

    public void PlayJumpSound()
    {
        if (jumpSound != null && mainAudioSource != null && !isPaused) 
            mainAudioSource.PlayOneShot(jumpSound);
    }

    public void HideUIForDialogue()
    {
        if (playerUIRoot != null) playerUIRoot.SetActive(false);
        DisableMovement();
    }

    public void ShowUIAfterDialogue()
    {
        if (playerUIRoot != null) playerUIRoot.SetActive(true);
        EnableMovement();
    }

    public void SetNewGameIntent()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 1);
        PlayerPrefs.Save();
    }
}