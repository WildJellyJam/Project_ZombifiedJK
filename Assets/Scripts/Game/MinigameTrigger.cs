using UnityEngine;
using System.Collections;

public class MinigameTrigger : MonoBehaviour
{
    public MinigameManagerJr minigameManager;  // Assign in Inspector
    public float delayBeforeStart = 1.5f;
    private bool hasTriggered = false;

    public void TriggerMinigame()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(StartMinigameAfterDelay());
    }

    private IEnumerator StartMinigameAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        if (minigameManager != null)
        {
            minigameManager.StartMinigameSequence();

        }
        else
        {
            Debug.LogWarning("No MinigameManager assigned to MinigameTrigger!");
        }
    }

    private void Start()
    {
        // Optional: automatically start on scene load
        TriggerMinigame();
    }
}