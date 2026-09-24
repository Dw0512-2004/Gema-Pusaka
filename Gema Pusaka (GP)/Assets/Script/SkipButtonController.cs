using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipButtonController : MonoBehaviour
{
    public string targetSceneName = "tutorial Scene";

    public void SkipToNextScene()
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}