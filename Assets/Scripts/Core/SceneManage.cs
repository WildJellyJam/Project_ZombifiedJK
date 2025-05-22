using UnityEngine;
using UnityEngine.SceneManagement;
public static class SceneManage
{
    private static bool isLoading = false;

    public static void SwitchSceneBasedOnTime(TimePeriod period)
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
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
        
    }
    public static void SwitchScene(TimePeriod period)
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
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
    
    public static void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name != sceneName && !isLoading)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
    public static void ReturnToMainMenuScene()
    {
        SceneManager.LoadSceneAsync("StartUpMenu");
    }
}
