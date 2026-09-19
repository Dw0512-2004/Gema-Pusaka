using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))] // 这个 BoxCollider 必须勾选 Is Trigger!
public class GendangPlatformDetector : MonoBehaviour
{
    [Header("需要一同激活的平台")]
    [Tooltip("将这一区域内需要同时显现的平台物件拖入这个列表 (最多可以放 3 个或更多)")]
    public List<GendangPlatform> targetPlatforms;

    private bool isPlayerNear = false;
    private bool isActivated = false;

    private void OnEnable()  => SkillManager.OnGendangUsed += HandleSkill;
    
    private void OnDisable() 
    {
        SkillManager.OnGendangUsed -= HandleSkill;
        
        // 【优化加固】：当物体被禁用或切场景时，强行停止未完成的协程，防止状态残留
        StopAllCoroutines();
        isActivated = false;
        isPlayerNear = false;
    }

    // 检测玩家是否进入这个“施法大区”
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNear = false;
    }

    private void HandleSkill(float audioDuration)
    {
        if (isPlayerNear && !isActivated)
        {
            StartCoroutine(ActivateGroupSequence(audioDuration));
        }
    }

    private IEnumerator ActivateGroupSequence(float initialDelay)
    {
        isActivated = true;

        // 1. 等待音效播放完毕
        yield return new WaitForSeconds(initialDelay);

        // 2. 遍历列表，将所有平台一起激活
        foreach (var platform in targetPlatforms)
        {
            if (platform != null) platform.Activate();
        }

        // 3. 维持 5 秒
        yield return new WaitForSeconds(5f);

        // 4. 遍历列表，将所有平台一起关闭
        foreach (var platform in targetPlatforms)
        {
            if (platform != null) platform.Deactivate();
        }

        isActivated = false;
    }
}