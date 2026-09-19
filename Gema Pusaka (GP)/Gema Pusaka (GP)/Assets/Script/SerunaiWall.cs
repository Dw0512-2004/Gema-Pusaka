using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class SerunaiWall : MonoBehaviour
{
    private bool isPlayerNear = false;

    private void OnEnable()  => SkillManager.OnSerunaiUsed += HandleSkill;
    private void OnDisable() => SkillManager.OnSerunaiUsed -= HandleSkill;

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
            StartCoroutine(WaitAndDisappear(audioDuration));
        }
    }

    private IEnumerator WaitAndDisappear(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // 直接隐藏整个墙壁
    }
}