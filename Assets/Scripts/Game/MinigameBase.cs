using System;
using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    public event Action<bool> OnMinigameEnd;

    public void EndMinigame(bool won)
    {
        Debug.Log("Minigame ended: " + (won ? "WIN" : "LOSE"));
        OnMinigameEnd?.Invoke(won);
        Destroy(gameObject); // optional cleanup
    }
}