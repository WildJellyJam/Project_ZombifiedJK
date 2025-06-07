using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiniGameManager
{
    public static void StartMiniGame(string miniGameName)
    {
        if (miniGameName == "DodgeHand" && newGameManager.Instance.playerStats.anxiety > 50)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DodgeHandScene");
        }
        else if (miniGameName == "Breathing" && newGameManager.Instance.playerStats.anxiety > 80)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BreathingScene");
        }
    }

    public static void EndMiniGame(string result)
    {
        if (result == "Success")
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(-10f); // 成功降低焦慮
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}