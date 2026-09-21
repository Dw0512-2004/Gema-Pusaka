using UnityEngine;

public class CameraFittoScreen : MonoBehaviour
{
    private Camera cam;
    public float targetAspect = 16.0f / 9.0f; // 你设计时期望的宽高比（例如 16:9）
    public float baseOrthographicSize = 5.0f;  // 在该宽高比下的相机 Size

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    void Update()
    {
        // 如果是在编辑器中测试改变窗口大小，可以放在 Update 实时生效；正式发布可以只在 Start 运行
        AdjustCamera();
    }

    void AdjustCamera()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect >= targetAspect)
        {
            // 当前屏幕比设计时更宽，保持高度不变，视野自动向两边扩展
            cam.orthographicSize = baseOrthographicSize;
        }
        else
        {
            // 当前屏幕比设计时更窄（或更高），自动放大相机视野，保证左右内容不会被裁剪
            cam.orthographicSize = baseOrthographicSize * (targetAspect / currentAspect);
        }
    }
}