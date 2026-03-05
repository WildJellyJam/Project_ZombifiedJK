using UnityEngine;
using UnityEngine.UI;

public class PressButtonToWin : MinigameBase
{
    [Header("UI Button (Optional)")]
    [Tooltip("Assign a Unity UI Button. Clicking it will win the minigame.")]
    public Button winButton;

    [Header("Keyboard Shortcut (Optional)")]
    [Tooltip("If not None, pressing this key will also win the minigame.")]
    public KeyCode winKey = KeyCode.Space;

    private bool hasEnded = false;

    private void Start()
    {
        // Hook up the button if it exists
        if (winButton != null)
        {
            winButton.onClick.AddListener(OnWinButtonPressed);
        }
        else
        {
            Debug.LogWarning("[PressButtonToWin] No winButton assigned. You can still use the keyboard key.");
        }
    }

    private void Update()
    {
        if (hasEnded) return;

        // Keyboard shortcut to win (e.g. Space)
        if (winKey != KeyCode.None && Input.GetKeyDown(winKey))
        {
            Debug.Log($"[PressButtonToWin] Win key pressed: {winKey}");
            Win();
        }
    }

    // Called by UI Button or keyboard
    public void OnWinButtonPressed()
    {
        if (hasEnded) return;

        Debug.Log("[PressButtonToWin] UI button clicked → player wins.");
        Win();
    }

    private void Win()
    {
        if (hasEnded) return;

        hasEnded = true;

        // Call your usual minigame system
        EndMinigame(true);
    }
}
