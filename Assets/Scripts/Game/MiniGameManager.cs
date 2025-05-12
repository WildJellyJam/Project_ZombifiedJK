using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiniGameManager
{
    public static void StartMiniGame(string miniGameName)
    {
        if (miniGameName == "DodgeHand" && PlayerStats.anxiety > 50)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DodgeHandScene");
        }
        else if (miniGameName == "Breathing" && PlayerStats.anxiety > 80)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BreathingScene");
        }
    }

    public static void EndMiniGame(string result)
    {
        if (result == "Success")
        {
            PlayerStats.UpdateAnxiety(-10f); // 成功降低焦慮
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}