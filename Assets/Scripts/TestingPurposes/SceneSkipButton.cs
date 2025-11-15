using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSkipButton : MonoBehaviour
{
    [Header("Name of the scene you want to skip to")]
    public string targetSceneName = "4_atSchool";

    public void SkipScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneSkipButton: No scene name provided!");
            return;
        }

        Debug.Log("⏩ Skipping to: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }
}
