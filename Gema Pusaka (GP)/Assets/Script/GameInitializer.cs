using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class GameInitializer : MonoBehaviour
{
    [Header("全局传送网关")]
    [Tooltip("跨场景传送时记录的目标出生点 ID（由 SceneTransition 写入）")]
    public static string nextSpawnPoint = "";

    [Header("默认出生点设置 (用于读档或直接进游戏)")]
    [Tooltip("如果没有指定 nextSpawnPoint，则默认寻找这个名字的出生点")]
    public string defaultSpawnPointName = "Spawn_Start";

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
        if (NusaController.Instance == null && nusaPrefab != null)
        {
            Vector3 spawnPos = Vector3.zero;

            // 如果是通过传送门切过来的，先暂时在原点生成，交由后面的 PlayerSpawnPoint 精准定位
            // 如果是读档进来的，则尝试读取存档坐标
            bool hasSavedPos = PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1;
            if (string.IsNullOrEmpty(nextSpawnPoint) && hasSavedPos)
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
            // 如果跨场景带过来的 Nusa 已经存在，且不是通过传送门（比如读档），检查是否需要传送回存档位置
            bool hasSavedPos = PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1;
            if (string.IsNullOrEmpty(nextSpawnPoint) && hasSavedPos)
            {
                float posX = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosX", 0f);
                float posY = PlayerPrefs.GetFloat("Slot_" + activeSlot + "_PosY", 0f);
                NusaController.Instance.transform.position = new Vector3(posX, posY, 0f);
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
        // 1. 锁住移动并清空物理速度
        if (NusaController.Instance != null)
        {
            NusaController.Instance.canMove = false;
            Rigidbody2D rb = NusaController.Instance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // 2. 等待一帧，让场景中所有的 PlayerSpawnPoint（出生点）完成 Start() 逻辑
        yield return null;

        // 3. 容错判断：如果没有通过传送门指定 nextSpawnPoint，并且也没有存档坐标，则寻找默认出生点
        if (string.IsNullOrEmpty(nextSpawnPoint))
        {
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);
            bool hasSavedPos = PlayerPrefs.GetInt("Slot_" + activeSlot + "_HasSavedPos", 0) == 1;

            if (!hasSavedPos)
            {
                GameObject spawnPoint = GameObject.Find(defaultSpawnPointName);
                if (spawnPoint == null) spawnPoint = GameObject.Find("SpawnPoint");
                if (spawnPoint == null) spawnPoint = GameObject.Find("Start Point");

                if (spawnPoint != null && NusaController.Instance != null)
                {
                    Vector3 targetPos = spawnPoint.transform.position;
                    targetPos.z = NusaController.Instance.transform.position.z;
                    NusaController.Instance.transform.position = targetPos;
                    Debug.Log($"<color=green>【默认出生】未指定传送门ID，已传送到默认出生点: {spawnPoint.name}</color>");
                }
            }
        }

        // 4. 绑定 Cinemachine 相机，瞬间切过去防止远距离飞掠
        CinemachineCamera vcam = Object.FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null && NusaController.Instance != null)
        {
            vcam.Follow = NusaController.Instance.transform;
            vcam.OnTargetObjectWarped(NusaController.Instance.transform, NusaController.Instance.transform.position - vcam.transform.position);
        }

        // 5. 恢复控制权
        if (NusaController.Instance != null && string.IsNullOrEmpty(nextSpawnPoint))
        {
            NusaController.Instance.canMove = true;
            Time.timeScale = 1f;
        }

        Debug.Log("<color=green>【GameManager】关卡初始化与坐标对齐完毕！</color>");
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