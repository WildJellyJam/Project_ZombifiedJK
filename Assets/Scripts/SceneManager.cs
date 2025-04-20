using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public void SwitchSceneBasedOnTime(TimePeriod period)
    {
        string sceneName = period switch
        {
            
            //TimePeriod.MorningSchool => "MorningSchoolScene",
            
            //TimePeriod.MorningSchool => "MorningSchoolScene",
             //TimePeriod.MorningSchool => "MorningHomeScene",
             TimePeriod.MorningHome => "MorningHomeScene",
            _ => "MorningHomeScene"
        };
        //UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress}");
            yield return null;
        }
    }
}
