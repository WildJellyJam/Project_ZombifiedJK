using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

[System.Serializable]
public class MinigameOption
{
    [Header("Anxiety Range")]
    [Tooltip("Minimum anxiety value (inclusive) for this minigame.")]
    public float minAnxiety;
    [Tooltip("Maximum anxiety value (exclusive) for this minigame.")]
    public float maxAnxiety;

    [Header("Minigame Prefab")]
    [Tooltip("Prefab to spawn for this minigame.")]
    public GameObject minigamePrefab;

    [Header("Cutscenes")]
    public VideoClip introVideo;
    public VideoClip winVideo;
    public VideoClip loseVideo;
    public float timeLimitSeconds = 15f;
}

public class MinigameManagerJr : MonoBehaviour
{
    [Header("Main Settings")]
    [Tooltip("How many minigames to play before finishing.")]
    public int minigamesToPlay = 3;
    private int currentMinigameCount = 0;
    private GameObject activeMinigame;

    [Header("Video Player")]
    [Tooltip("The Video Player component used for all cutscenes.")]
    public VideoPlayer videoPlayer;

    [Header("Available Minigames")]
    public List<MinigameOption> minigameOptions = new List<MinigameOption>();

    // ======================  LIFE CYCLE  =======================
    private IEnumerator Start()
    {
        // Wait until newGameManager exists and playerStats initialized
        yield return new WaitUntil(() => newGameManager.Instance != null && newGameManager.Instance.playerStats != null);
        Debug.Log($"🧠 newGameManager found! Anxiety = {newGameManager.Instance.playerStats.anxiety}");

        StartMinigameSequence();
    }

    // ======================  ENTRY POINT  ======================
    public void StartMinigameSequence()
    {
        currentMinigameCount = 0;
        StartCoroutine(PlayMinigameFlow());
    }

    // ======================  MAIN FLOW  ========================
    private IEnumerator PlayMinigameFlow()
    {
        Debug.Log("▶️ Starting PlayMinigameFlow()");
        currentMinigameCount = 0;

        while (currentMinigameCount < minigamesToPlay)
        {
            Debug.Log($"--- Round {currentMinigameCount + 1} ---");

            // 1) Select option
            MinigameOption option = SelectMinigameBasedOnAnxiety();
            if (option == null)
            {
                Debug.LogError("❌ No MinigameOption returned!");
                yield break;
            }

            if (option.minigamePrefab == null)
            {
                Debug.LogError("❌ option.minigamePrefab is NULL in Inspector!");
                yield break;
            }

            // 2) Intro video
            if (option.introVideo != null)
                yield return PlayCutscene(option.introVideo);

            // 3) Instantiate
            activeMinigame = Instantiate(option.minigamePrefab);

            // ✅ IMPORTANT: search in children too
            MinigameBase minigame = activeMinigame.GetComponentInChildren<MinigameBase>(true);
            if (minigame == null)
            {
                Debug.LogError($"❌ The prefab '{option.minigamePrefab.name}' has NO MinigameBase (on root or children).");
                Destroy(activeMinigame);
                activeMinigame = null;
                yield break;
            }

            // ✅ If this minigame supports timer, set time limit from option
            TimedMinigameBase timed = activeMinigame.GetComponentInChildren<TimedMinigameBase>(true);
            if (timed != null)
            {
                timed.SetTimeLimit(option.timeLimitSeconds);
            }

            bool? result = null;
            minigame.OnMinigameEnd += (bool won) => { result = won; };

            // Wait for minigame to call EndMinigame(...)
            yield return new WaitUntil(() => result.HasValue);

            Debug.Log($"🏁 Minigame finished, result = {result.Value}");

            // 4) Win / Lose video
            if (result.Value && option.winVideo != null)
                yield return PlayCutscene(option.winVideo);
            else if (!result.Value && option.loseVideo != null)
                yield return PlayCutscene(option.loseVideo);

            // 5) Cleanup
            Destroy(activeMinigame);
            activeMinigame = null;

            currentMinigameCount++;
        }

        FinishAllMinigames();
    }

    // ====================  CUTSCENE HANDLER  ===================
    private IEnumerator PlayCutscene(VideoClip clip)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("❌ VideoPlayer not assigned to MinigameManagerJr!");
            yield break;
        }
        if (clip == null)
        {
            Debug.LogError("❌ Video clip is null!");
            yield break;
        }

        RawImage raw = videoPlayer.GetComponent<RawImage>();
        if (raw != null)
        {
            raw.enabled = true;
            raw.color = Color.white;
        }

        videoPlayer.clip = clip;
        videoPlayer.Play();
        Debug.Log($"▶️ Playing {clip.name}...");

        // Wait up to 2 seconds for the first frame
        float waitTimer = 0f;
        while (!videoPlayer.isPlaying && waitTimer < 2f)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }
        Debug.Log(videoPlayer.isPlaying ? "✅ Video started." : "⚠️ Video never started.");

        while (videoPlayer.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                videoPlayer.Stop();
                break;
            }
            yield return null;
        }

        Debug.Log("🎬 Video ended.");
        yield return new WaitForSeconds(0.3f);
        if (raw != null) raw.enabled = false;
    }


    // ====================  FINISH SEQUENCE  ====================
    private void FinishAllMinigames()
    {
        Debug.Log($"✅ All {minigamesToPlay} minigames complete!");
        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
        {
            // 使用 PlayerStats 的 UpdateAnxiety 以保持邊界與壞結局邏輯
            newGameManager.Instance.playerStats.UpdateAnxiety(-10f);
        }
        // Example: SceneManager.LoadScene("NextSceneName");
    }

    // ====================  SELECT MINIGAME  ====================
    private MinigameOption SelectMinigameBasedOnAnxiety()
    {
        if (newGameManager.Instance == null || newGameManager.Instance.playerStats == null)
        {
            Debug.LogError("❌ newGameManager.Instance or playerStats is null!");
            return null;
        }

        float anxiety = newGameManager.Instance.playerStats.anxiety;
        Debug.Log($"🔍 Selecting minigame for anxiety = {anxiety}");

        List<MinigameOption> validOptions = new List<MinigameOption>();
        foreach (var option in minigameOptions)
        {
            Debug.Log($"Checking option range {option.minAnxiety}–{option.maxAnxiety}");
            if (anxiety >= option.minAnxiety && anxiety < option.maxAnxiety)
                validOptions.Add(option);
        }

        if (validOptions.Count == 0)
        {
            Debug.LogWarning($"⚠ No minigame matches anxiety level {anxiety}. Defaulting to first option.");
            return minigameOptions.Count > 0 ? minigameOptions[0] : null;
        }

        int index = Random.Range(0, validOptions.Count);
        Debug.Log($"✅ Selected minigame index {index}");
        return validOptions[index];
    }
}
