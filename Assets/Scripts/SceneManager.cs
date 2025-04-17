using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public void SwitchSceneBasedOnTime(TimePeriod period)
    {
        string sceneName = period switch
        {
            TimePeriod.MorningSchool => "MorningSchoolScene",
            TimePeriod.MorningHome => "MorningHomeScene",
            //TimePeriod.MorningSchool => "MorningSchoolScene",
            _ => "MainScene"
        };
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
