using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class GameInitializer : MonoBehaviour
{
    [Header("全局传送网关 (原 GameManager 功能)")]
    [Tooltip("跨场景传送时记录的目标出生点 ID（由 ScenePortal 写入）")]
    public static string nextSpawnPoint = "";

    [Header("默认出生点设置 (用于读档或直接进游戏)")]
    [Tooltip("如果没有指定 nextSpawnPoint，则默认寻找这个名字的出生点")]
    public string defaultSpawnPointName = "Start Point";

    [Header("预制体与容错")]
    [Tooltip("将你的 Nusa 预制体 (Prefab) 拖到这里")]
    public GameObject nusaPrefab;

    private void Awake()
    {
        Debug.Log("<color=yellow>【GameManager】Awake 触发：关卡初始化启动...</color>");

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);

        // 1. 核心：生成或恢复 Nusa 玩家
        InitializePlayer(activeSlot);

        // 2. 核心：读取并恢复技能解锁状态
        InitializeSkills(activeSlot);
    }

    private void Start()
    {
        // 3. 核心：位置对齐与相机绑定
        StartCoroutine(PositionAndCameraSequence());
    }

    private void InitializePlayer(int activeSlot)
    {
        bool hasSavedPos = PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1;
        bool isNewGame = PlayerPrefs.GetInt("IsNewGame_Intent", 0) == 1;
        
        // 【關鍵防禦】：檢查車票。如果有車票，代表是過圖進來的，絕對不要讀取舊存檔座標！
        bool isFromPortal = !string.IsNullOrEmpty(nextSpawnPoint);

        if (NusaController.Instance == null && nusaPrefab != null)
        {
            Vector3 spawnPos = Vector3.zero;

            // 如果不是從傳送門來（例如直接開啟遊戲或讀檔），且有存檔紀錄
            if (!isFromPortal && hasSavedPos && !isNewGame)
            {
                float posX = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosX", 0f);
                float posY = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosY", 0f);
                spawnPos = new Vector3(posX, posY, 0f);
            }

            GameObject newNusa = Instantiate(nusaPrefab, spawnPos, Quaternion.identity);
            DontDestroyOnLoad(newNusa);
            Debug.Log("<color=green>【GameManager】成功实例化 Nusa 并设为 DontDestroyOnLoad。</color>");
        }
        else if (NusaController.Instance != null)
        {
            // 如果跨场景带过来的 Nusa 已经存在，且不是通过传送门过來的，才传送到存档位置
            if (!isFromPortal && hasSavedPos && !isNewGame)
            {
                float posX = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosX", 0f);
                float posY = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosY", 0f);
                NusaController.Instance.transform.position = new Vector3(posX, posY, 0f);
                Debug.Log($"<color=yellow>【GameManager】讀取存檔，覆寫座標至 ({posX}, {posY})</color>");
            }
        }
    }

    private void InitializeSkills(int activeSlot)
    {
        for (int s = 0; s < 4; s++)
        {
            int isUnlocked = PlayerPrefs.GetInt("Slot_" + activeSlot + "_Skill_" + s, 0);
            PlayerPrefs.SetInt("Runtime_Has_" + GetSkillNameKey(s), isUnlocked);
        }
    }

    private IEnumerator PositionAndCameraSequence()
    {
        if (NusaController.Instance != null)
        {
            NusaController.Instance.DisableMovement();
            Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 等待一帧，让场景中所有的 PlayerSpawnPoint 优先完成传送对齐逻辑
        yield return null;

        bool isNewGame = PlayerPrefs.GetInt("IsNewGame_Intent", 0) == 1;
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
        bool hasSavedPos = PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1;

        // 如果 nextSpawnPoint 为空，说明不是从传送门进来的，执行默认出生点逻辑
        if (string.IsNullOrEmpty(nextSpawnPoint))
        {
            if (isNewGame || !hasSavedPos)
            {
                GameObject spawnPoint = GameObject.Find(defaultSpawnPointName);
                if (spawnPoint == null) spawnPoint = GameObject.Find("SpawnPoint");
                if (spawnPoint == null) spawnPoint = GameObject.Find("Spawn_Start");

                if (spawnPoint != null && NusaController.Instance != null)
                {
                    Vector3 targetPos = spawnPoint.transform.position;
                    targetPos.z = NusaController.Instance.transform.position.z;
                    NusaController.Instance.transform.position = targetPos;
                    Debug.Log($"<color=green>【默认出生】未指定传送门ID，已传送到默认出生点: {spawnPoint.name}</color>");
                }
                else
                {
                    Debug.LogError("<color=red>【错误】场景中没有找到名为 '" + defaultSpawnPointName + "' 的起点！</color>");
                }

                if (isNewGame)
                {
                    PlayerPrefs.SetInt("IsNewGame_Intent", 0);
                    PlayerPrefs.Save();
                }
            }
        }

        // 绑定 Cinemachine 相机
        CinemachineCamera vcam = Object.FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null && NusaController.Instance != null)
        {
            vcam.Follow = NusaController.Instance.transform;
            vcam.OnTargetObjectWarped(NusaController.Instance.transform, NusaController.Instance.transform.position - vcam.transform.position);
        }

        // 🚨 核心配合 1：强制等待 2 帧！
        // 这两帧是留给 Cinemachine 运算跟 Unity 渲染管线把“相机瞬移后的画面”画出来的时间。
        yield return null;
        yield return null;

        if (NusaController.Instance != null)
        {
            NusaController.Instance.EnableMovement();
            Time.timeScale = 1f;
        }
        
        // 【统一撕车票】：在一切都就绪后，由 GameManager 统一清空车票
        nextSpawnPoint = "";

        Debug.Log("<color=green>【GameManager】关卡初始化与坐标对齐完毕！</color>");

        // 🚨 核心配合 2：亲自下令解除 Loading 黑屏！
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.HideLoadingScreen();
        }
    }

    private string GetSkillNameKey(int index)
    {
        switch (index)
        {
            case 0: return "Serunai";
            case 1: return "Gong";
            case 2: return "Gendang";
            case 3: return "Nobat";
            default: return "";
        }
    }
}