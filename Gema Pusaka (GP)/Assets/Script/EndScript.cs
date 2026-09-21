using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndScript : MonoBehaviour
{
    [Header("主選單場景的名稱")]
    public string mainMenuSceneName = "MainMenu"; // 請確保這裡的名稱與你 Build Settings 裡的場景名稱一致

    // 這個方法用來給 Fungus 的 Invoke Method 呼叫
    public void OnEndingBlockFinished()
    {
        StartCoroutine(WaitAndLoadMainMenu());
    }

    private IEnumerator WaitAndLoadMainMenu()
    {
        // 延遲 2 秒
        yield return new WaitForSeconds(2f);
        
        // 載入主選單場景
        SceneManager.LoadScene(mainMenuSceneName);
    }
}