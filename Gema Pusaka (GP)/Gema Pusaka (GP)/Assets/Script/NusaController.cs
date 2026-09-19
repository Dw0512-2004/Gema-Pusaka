using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

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

    // --- Pause Menu UI 引用 (挂在 Nusa Prefab 上的 Canvas 子物体中) ---
    [Header("Pause Menu UI (挂在 Nusa 身上的 UI)")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject mapPanel;
    public string mainMenuSceneName = "Main_Menu";
    public string loadDataSceneName = "Load_Data_Scene";
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
            DontDestroyOnLoad(gameObject); // Nusa 及其身上的 Canvas 跨场景永生！
        }
        else
        {
            Destroy(gameObject);
            return; 
        }

        rb = GetComponent<Rigidbody2D>();
        an = GetComponent<Animator>();

        // 初始化关闭所有暂停菜单
        CloseAllPanels();
    }

    private void Update()
    {
        // ==========================================
        // 暂停菜单 / ESC 键逻辑
        // ==========================================
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

        if (isPaused) return; // 如果游戏暂停了，不处理下方移动和交互输入

        // 1. 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 安全坐标记录逻辑
        if (isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
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

        if (an != null) 
        {
            an.SetBool("isGrounded", isGrounded);
        }

        // ==========================================
        // 交互按键计时逻辑 (长按呼出技能面板)
        // ==========================================
        if (isInteractPressed && !panelToggled)
        {
            // 【关键修改】：检查是否解锁了技能，没有的话直接不计时
            if (SkillManager.Instance != null && !SkillManager.Instance.HasAnySkillUnlocked())
            {
                // 未解锁技能，直接忽略，不执行计时
            }
            else
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

        // ==========================================
        // 移动输入逻辑
        // ==========================================
        if (!canMove)
        {
            horizontalInput = 0f;
            if (an != null) an.SetFloat("Speed", 0f);
            return;
        }

        bool leftInput = isMovingLeft || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightInput = isMovingRight || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (leftInput && !rightInput) horizontalInput = -1f;
        else if (rightInput && !leftInput) horizontalInput = 1f;
        else horizontalInput = 0f;

        if (an != null) an.SetFloat("Speed", Mathf.Abs(horizontalInput));

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
    
    // ==========================================
    // 暂停菜单功能方法
    // ==========================================

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        DisableMovement();
        OpenPauseMenuOnly();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        EnableMovement();
        CloseAllPanels();
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

        // 保存技能
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_0", PlayerPrefs.GetInt("Runtime_Has_Serunai", 0));
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_1", PlayerPrefs.GetInt("Runtime_Has_Gong", 0));
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_2", PlayerPrefs.GetInt("Runtime_Has_Gendang", 0));
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_Skill_3", PlayerPrefs.GetInt("Runtime_Has_Nobat", 0));

        PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosX", transform.position.x);
        PlayerPrefs.SetFloat("Slot_" + activeSlot + "_PosY", transform.position.y);
        PlayerPrefs.SetInt("Slot_" + activeSlot + "_HasSavedPos", 1);

        PlayerPrefs.Save();
        Debug.Log($"<color=green>【快速存档成功】存至槽位 {activeSlot}！场景: {currentScene}</color>");

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

            // 如果退回主菜单，销毁 Nusa 避免带进主菜单
            if (targetSceneName == mainMenuSceneName)
            {
                Destroy(gameObject);
                Instance = null;
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

    // ==========================================
    // 移动与交互控制方法
    // ==========================================

    public void Jump()
    {
        if (isGrounded && canMove && !isPaused)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJumpForce);
            if (an != null) an.SetTrigger("Jump");
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
        
        // 只有在未呼出面板（短按）且且计时小于要求时，才触发技能释放
        if (!panelToggled && holdTimer < holdDurationRequired)
        {
            if (SkillManager.Instance != null) 
            {
                SkillManager.Instance.CastCurrentSkill(); // CastCurrentSkill 内部也有安全拦截
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
    // Fungus 集成方法
    // ==========================================
    
    // 供 Fungus 的 Invoke Method 调用，告诉系统这是一场新游戏
    public void SetNewGameIntent()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 1);
        PlayerPrefs.Save();
        Debug.Log("<color=cyan>【Nusa】已标记为新游戏，将无视旧存档坐标！</color>");
    }
}