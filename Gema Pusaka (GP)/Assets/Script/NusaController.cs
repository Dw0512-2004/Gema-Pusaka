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
    [Tooltip("最后一次安全的地面坐标")]
    public Vector3 lastSafePosition;
    [Tooltip("在地面上站立多久才算作安全位置 (防悬崖边缘滑落)")]
    public float timeToRecordSafePos = 0.2f;
    private float groundedTimer = 0f;
    
    [Header("Interaction Settings (交互设置)")]
    [Tooltip("长按多少秒后呼出技能面板")]
    [SerializeField] private float holdDurationRequired = 0.5f;

    // --- 音效設定 ---
    [Header("Audio Settings (音效設定)")]
    [SerializeField] private AudioClip footstepSound; // 脚步声音效 (建议使用循环的脚步音效)
    [SerializeField] private AudioClip jumpSound;     // 跳跃声音效
    private AudioSource audioSource;

    // --- UI 引用 ---
    [Header("UI References (UI 引用)")]
    [Tooltip("挂在 Nusa 身上的 Canvas 或总 UI 根物体，对话时会自动隐藏整套 UI")]
    public GameObject playerUIRoot;
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject mapPanel;

    [Header("Scene Settings (场景设置)")]
    public string mainMenuSceneName = "Main_Menu";
    public string loadDataSceneName = "Load_Data_Scene";
    public string introductionSceneName = "Introduction_Scene"; 
    [Tooltip("结局动画场景的名称")]
    public string endSceneName = "End_Scene"; 

    [HideInInspector] public bool isPaused = false;

    // --- 移动端虚拟按键状态 ---
    private bool isMovingLeft;
    private bool isMovingRight;
    private float horizontalInput;

    // --- 交互按键状态 ---
    private bool isInteractPressed = false;
    private float holdTimer = 0f;
    private bool panelToggled = false; 

    // --- 组件引用 ---
    private Rigidbody2D rb;
    private Animator an;
    private Vector2 moveVelocity;
    
    // --- 状态变量 ---
    [Header("Current State (当前状态)")]
    public bool isGrounded;
    public bool isFacingRight = true;
    public bool canMove = true; 

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
        audioSource = GetComponent<AudioSource>(); 

        CloseAllPanels();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == endSceneName || 
            scene.name == mainMenuSceneName || 
            scene.name == loadDataSceneName || 
            scene.name == introductionSceneName)
        {
            Time.timeScale = 1f; 
            if (Instance == this)
            {
                Instance = null;
            }
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    OpenPauseMenuOnly();
                }
                else if (mapPanel != null && mapPanel.activeSelf)
                {
                    CloseMap();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }

        if (isPaused) return; 

        Collider2D hitCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = hitCollider != null;
        
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            if (hitCollider.gameObject.layer != LayerMask.NameToLayer("HiddenPlatform"))
            {
                groundedTimer += Time.deltaTime;
                if (groundedTimer >= timeToRecordSafePos)
                {
                    lastSafePosition = transform.position;
                }
            }
            else
            {
                groundedTimer = 0f;
            }
        }
        else
        {
            groundedTimer = 0f; 
        }

        if (an != null) 
        {
            an.SetBool("isGrounded", isGrounded);
        }

        if (isInteractPressed && !panelToggled)
        {
            if (SkillManager.Instance == null || SkillManager.Instance.HasAnySkillUnlocked())
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdDurationRequired)
                {
                    if (SkillManager.Instance != null)
                    {
                        SkillManager.Instance.ShowPanel();
                        panelToggled = true; 
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E)) SkillPointerDown();
        if (Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.E)) SkillPointerUp();

        if (!canMove)
        {
            horizontalInput = 0f;
            if (an != null) an.SetFloat("Speed", 0f);
            StopFootstepSound();
            return;
        }

        bool leftInput = isMovingLeft || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightInput = isMovingRight || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (leftInput && !rightInput) horizontalInput = -1f;
        else if (rightInput && !leftInput) horizontalInput = 1f;
        else horizontalInput = 0f;

        if (an != null) an.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // ==========================================
        // 🌟 走路音效自動控管 (替代動畫事件)
        // ==========================================
        bool isWalking = isGrounded && Mathf.Abs(horizontalInput) > 0.01f && canMove;

        if (isWalking)
        {
            if (footstepSound != null && audioSource != null && audioSource.clip != footstepSound)
            {
                audioSource.clip = footstepSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (audioSource != null && !audioSource.isPlaying && audioSource.clip == footstepSound)
            {
                audioSource.Play();
            }
        }
        else
        {
            // 一旦停下腳步、或離地跳躍，立即中斷腳步聲
            StopFootstepSound();
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

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        EnableMovement();
        CloseAllPanels();

        if (audioSource != null)
        {
            audioSource.UnPause();
        }
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

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void CloseMap()
    {
        ResumeGame();
    }

    public void SaveGameDataToCurrentSlot()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("Slot_" + activeSlot + "_Scene", currentScene);

        int currentProgress = PlayerPrefs.GetInt("Runtime_GameProgress", 0); 
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Progress", currentProgress == 0 ? 10 : currentProgress);

        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_0", PlayerPrefs.GetInt("Runtime_Has_Serunai", 0));
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_1", PlayerPrefs.GetInt("Runtime_Has_Gong", 0));
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_2", PlayerPrefs.GetInt("Runtime_Has_Gendang", 0));
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_3", PlayerPrefs.GetInt("Runtime_Has_Nobat", 0));

        PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosX", transform.position.x);
        PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosY", transform.position.y);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_HasSavedPos", 1);

        PlayerPrefs.Save();
        ResumeGame();
    }

    public void GoToLoadDataScene()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 0);
        PlayerPrefs.Save();
        SafeExitToScene(loadDataSceneName);
    }

    public void ExitToMainMenu()
    {
        SafeExitToScene(mainMenuSceneName);
    }

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

            if (targetSceneName == mainMenuSceneName || 
                targetSceneName == loadDataSceneName || 
                targetSceneName == introductionSceneName ||
                targetSceneName == endSceneName)
            {
                if (Instance == this)
                {
                    Instance = null;
                }
                Destroy(gameObject);
            }
        };

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadSceneWithCleanUp(targetSceneName, cleanUpLogic);
        }
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
            
            // 起跳時立即斷掉腳步聲，並播放跳躍音效
            StopFootstepSound();
            PlayJumpSound();
        }
    }

    public void StopJump()
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
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
        StopFootstepSound();
    }

    public void EnableMovement()
    {
        canMove = true;
    }

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
        
        if (!panelToggled && holdTimer < holdDurationRequired)
        {
            if (SkillManager.Instance != null) 
            {
                SkillManager.Instance.CastCurrentSkill(); 
            }
        }
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

    // ==========================================
    // 音效管理方法
    // ==========================================
    
    private void StopFootstepSound()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == footstepSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void PlayJumpSound()
    {
        if (jumpSound != null && audioSource != null && !isPaused)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // ==========================================
    // Fungus 集成方法
    // ==========================================
    
    public void HideUIForDialogue()
    {
        if (playerUIRoot != null)
        {
            playerUIRoot.SetActive(false);
        }
        DisableMovement();
    }

    public void ShowUIAfterDialogue()
    {
        if (playerUIRoot != null)
        {
            playerUIRoot.SetActive(true);
        }
        EnableMovement();
    }

    public void SetNewGameIntent()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 1);
        PlayerPrefs.Save();
    }
}