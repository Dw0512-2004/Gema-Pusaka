using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(LineRenderer))]
public class GendangPlatform : MonoBehaviour
{
    private BoxCollider2D solidCollider;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;

    [Header("外观设置")]
    public Sprite solidSprite;  // 实体状态图
    
    [Header("虚线设置 (Line Renderer)")]
    public Material dashedMaterial; // 需要挂载一个带有虚线贴图的材质
    public float lineWidth = 0.05f; // 虚线粗细

    private void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        // 初始化
        solidCollider.isTrigger = false; 
        SetupLineRenderer();
        Deactivate();
    }

    private void SetupLineRenderer()
    {
        // 设置 LineRenderer 基础属性
        lineRenderer.positionCount = 5; // 4个角 + 回到起点闭合
        lineRenderer.useWorldSpace = false; // 相对自身坐标，平台移动时虚线会跟着走
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = dashedMaterial;
        
        // 关键设置：让材质平铺，从而形成虚线效果
        lineRenderer.textureMode = LineTextureMode.Tile; 

        // 获取 Collider 的大小与偏移
        Vector2 size = solidCollider.size;
        Vector2 offset = solidCollider.offset;
        float halfX = size.x / 2f;
        float halfY = size.y / 2f;

        // 根据 Collider 画出四个角的点
        Vector3[] positions = new Vector3[5];
        positions[0] = new Vector3(offset.x - halfX, offset.y - halfY, 0); // 左下
        positions[1] = new Vector3(offset.x - halfX, offset.y + halfY, 0); // 左上
        positions[2] = new Vector3(offset.x + halfX, offset.y + halfY, 0); // 右上
        positions[3] = new Vector3(offset.x + halfX, offset.y - halfY, 0); // 右下
        positions[4] = positions[0]; // 回到左下闭合

        lineRenderer.SetPositions(positions);
    }

    public void Activate()
    {
        spriteRenderer.sprite = solidSprite;
        spriteRenderer.enabled = true;      // 显示实体图
        lineRenderer.enabled = false;       // 隐藏虚线框
        solidCollider.enabled = true;       // 开启物理碰撞
    }

    public void Deactivate()
    {
        spriteRenderer.enabled = false;     // 隐藏实体图
        lineRenderer.enabled = true;        // 显示虚线框
        solidCollider.enabled = false;      // 关闭物理碰撞
    }
}