using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingFlow : MonoBehaviour
{
    public static EndingFlow Instance { get; private set; }

    [Header("Scene Names")]
    public string badInteractSceneName = "BadEndingScene";
    public string endingTextSceneName = "EndingTextScene";

    [Header("Runtime")]
    public EndingSequence pendingSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartBadEndingFlow(EndingSequence badSeq)
    {
        pendingSequence = badSeq;
        SceneManager.LoadScene(badInteractSceneName);
    }

    public void GoToEndingText()
    {
        SceneManager.LoadScene("10_BadEnding");
    }

    public void StartEndingText(EndingSequence seq)
    {
        pendingSequence = seq;
        SceneManager.LoadScene("10_BadEnding");
    }

    public void ClearPending()
    {
        pendingSequence = null;
    }
}