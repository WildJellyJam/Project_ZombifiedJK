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
        // Wait until GameManager exists
        yield return new WaitUntil(() => GameManager.Instance != null);
        Debug.Log($"🧠 GameManager found! Anxiety = {GameManager.Instance.anxiety}");

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

        while (currentMinigameCount < minigamesToPlay)
        {
            Debug.Log($"--- Round {currentMinigameCount + 1} ---");

            // 1️⃣ Select minigame
            MinigameOption option = SelectMinigameBasedOnAnxiety();

            if (option == null)
            {
                Debug.LogError("❌ SelectMinigameBasedOnAnxiety() returned null!");
                yield break;
            }

            Debug.Log($"✅ Selected minigame prefab: {(option.minigamePrefab ? option.minigamePrefab.name : "NULL")}");
            Debug.Log($"🎬 Intro clip: {(option.introVideo ? option.introVideo.name : "none")}");

            // 2️⃣ Play intro
            if (option.introVideo != null)
                yield return PlayCutscene(option.introVideo);

            // 3️⃣ Instantiate
            if (option.minigamePrefab == null)
            {
                Debug.LogError("❌ option.minigamePrefab is NULL in Inspector!");
                yield break;
            }

            activeMinigame = Instantiate(option.minigamePrefab);
            MinigameBase minigame = activeMinigame.GetComponentInChildren<MinigameBase>();

            if (minigame == null)
            {
                Debug.LogError($"❌ The prefab '{option.minigamePrefab.name}' has NO MinigameBase script!");
                yield break;
            }

            bool? result = null;
            minigame.OnMinigameEnd += (bool won) => { result = won; };

            yield return new WaitUntil(() => result.HasValue);

            Debug.Log($"🏁 Minigame finished, result = {result}");

            // 4️⃣ Play win / lose
            if (result.Value && option.winVideo != null)
                yield return PlayCutscene(option.winVideo);
            else if (!result.Value && option.loseVideo != null)
                yield return PlayCutscene(option.loseVideo);

            // 5️⃣ Clean up
            Destroy(activeMinigame);
            activeMinigame = null;
            currentMinigameCount++;
        }
        Debug.Log($"Spawned: {activeMinigame.name}, Has MinigameBase: {activeMinigame.GetComponent<MinigameBase>() != null}");

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
        GameManager.Instance.AddAnxiety(-10);
        // Example: SceneManager.LoadScene("NextSceneName");
    }

    // ====================  SELECT MINIGAME  ====================
    private MinigameOption SelectMinigameBasedOnAnxiety()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance is null!");
            return null;
        }

        float anxiety = GameManager.Instance.anxiety;
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
