using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private bool isLoading = false;
    public void SwitchSceneBasedOnTime(TimePeriod period)
    {
        string sceneName = period switch
        {
            TimePeriod.AtSchoolAfterClass => "0_atSchoolAfterClass",
            TimePeriod.AtHomeBeforeSleep => "1_atHomeBeforeSleep",
            TimePeriod.AtHomeAfterWakeUp => "2_atHomeAfterWakeUp",
            TimePeriod.AtHomeBeforeLeaving => "3_atHomeBeforeLeaving",
            TimePeriod.AtSchool => "4_atSchool",
            TimePeriod.AtHome => "5_atHome",
            TimePeriod.InCity => "6_inCity",
            TimePeriod.AtSupermarket => "7_atSupermarket",
            TimePeriod.AtHomeParentsArgue => "8_atHomeParentsArgue",
            TimePeriod.AtAdventure => "9_atAdventure",
            _ => "1_atHomeBeforeSleep"
        };
        //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
    }
    public void SwitchScene(TimePeriod period)
    {
        string sceneName = period switch
        {
            TimePeriod.AtSchoolAfterClass => "0_atSchoolAfterClass",
            TimePeriod.AtHomeBeforeSleep => "1_atHomeBeforeSleep",
            TimePeriod.AtHomeAfterWakeUp => "2_atHomeAfterWakeUp",
            TimePeriod.AtHomeBeforeLeaving => "3_atHomeBeforeLeaving",
            TimePeriod.AtSchool => "4_atSchool",
            TimePeriod.AtHome => "5_atHome",
            TimePeriod.InCity => "6_inCity",
            TimePeriod.AtSupermarket => "7_atSupermarket",
            TimePeriod.AtHomeParentsArgue => "8_atHomeParentsArgue",
            TimePeriod.AtAdventure => "9_atAdventure",
            _ => "1_atHomeBeforeSleep"
        };
        // 檢查當前場景是否與目標場景相同，避免重複加載
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneName && !isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true; // 標記正在加載
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        // 等待加載完成
        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
            yield return null;
        }

        isLoading = false; // 加載完成，重置標記
    }
    public void LoadScene(string sceneName)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneName && !isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }
}
