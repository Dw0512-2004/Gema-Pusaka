using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class GendangPlatform : MonoBehaviour
{
    private BoxCollider2D solidCollider;
    private SpriteRenderer spriteRenderer;

    [Header("外观设置")]
    public Sprite dashedSprite; // 虚线状态图
    public Sprite solidSprite;  // 实体状态图

    private void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化：确保它是非触发器（用于站立）并处于隐藏状态
        solidCollider.isTrigger = false; 
        Deactivate();
    }

    public void Activate()
    {
        spriteRenderer.sprite = solidSprite;
        solidCollider.enabled = true;
    }

    public void Deactivate()
    {
        spriteRenderer.sprite = dashedSprite;
        solidCollider.enabled = false;
    }
}