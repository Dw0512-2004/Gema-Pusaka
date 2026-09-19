using UnityEngine;

public class IntroHelper : MonoBehaviour
{
    // 这个方法专门留给 Fungus 调用
    public void MarkNewGame()
    {
        PlayerPrefs.SetInt("IsNewGame_Intent", 1);
        PlayerPrefs.Save();
        Debug.Log("<color=cyan>【Fungus】已成功标记为新游戏！准备跳转并空投到 Spawn_Start！</color>");
    }
}