using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public void SwitchSceneBasedOnTime(TimePeriod period)
    {
        string sceneName = period switch
        {
            TimePeriod.MorningHome => "MorningHomeScene",
            TimePeriod.MorningSchool => "MorningSchoolScene",
            
            //TimePeriod.MorningSchool => "MorningSchoolScene",
             //TimePeriod.StartMenu => "StartUpMenu",
            _ => "MainScene"
        };
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
