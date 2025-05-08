using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviour
{
    public void StartMiniGame(string miniGameName, PlayerStats playerStats)
    {
        if (miniGameName == "DodgeHand" && playerStats.anxiety > 50)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DodgeHandScene");
        }
        else if (miniGameName == "Breathing" && playerStats.anxiety > 80)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BreathingScene");
        }
    }

    public void EndMiniGame(string result, PlayerStats playerStats)
    {
        if (result == "Success")
        {
            playerStats.UpdateAnxiety(-10f); // 成功降低焦慮
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}