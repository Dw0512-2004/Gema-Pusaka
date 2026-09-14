using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class GongObstacles : MonoBehaviour
{
    private bool isPlayerNear = false;

    private void OnEnable()  => SkillManager.OnGongUsed += HandleSkill;
    private void OnDisable() => SkillManager.OnGongUsed -= HandleSkill;

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
        if (isPlayerNear)
        {
            StartCoroutine(WaitAndDestroy(audioDuration));
        }
    }

    private IEnumerator WaitAndDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 这里后续可以加入粒子特效 Instantiate(breakParticles, transform.position, Quaternion.identity);
        Destroy(gameObject); // 彻底销毁物体
    }
}