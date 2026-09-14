using UnityEngine;
using UnityEngine.UI; // 必须引入 UI 命名空间

public class VolumeController : MonoBehaviour
{
    [Header("音量滑块引用")]
    [Tooltip("将你 UI 里的 Slider 拖入此处")]
    public Slider volumeSlider;

    private void Start()
    {
        // 1. 读取本地保存的音量，如果没有存档，默认给最大音量 1.0f
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);

        // 2. 同步 UI 滑块的位置
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        // 3. 实际改变游戏的全局音量
        AudioListener.volume = savedVolume;
    }

    /// <summary>
    /// 当玩家拖动滑块时，Slider 会自动调用这个方法并传入当前的数值
    /// </summary>
    public void OnVolumeChanged(float value)
    {
        // 1. 实时改变全局音量 (范围是 0.0 到 1.0)
        AudioListener.volume = value;

        // 2. 保存进系统，确保跨场景（主菜单和游戏内）数据互通
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
}