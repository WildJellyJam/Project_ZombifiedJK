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
    public float minAnxiety;
    public float maxAnxiety;

    [Header("Minigame Prefab")]
    public GameObject minigamePrefab;

    [Header("Cutscenes")]
    public VideoClip introVideo;
    public VideoClip winVideo;
    public VideoClip loseVideo;
    public float timeLimitSeconds = 15f;
}

[System.Serializable]
public class SpecialDaySceneRule
{
    [Tooltip("第幾天要改跳特殊 scene")]
    public int day;

    [Tooltip("這一天完成小遊戲後要跳去的 scene 名稱")]
    public string sceneName;
}

public class MinigameManagerJr : MonoBehaviour
{
    [Header("Main Settings")]
    public int minigamesToPlay = 3;
    private int currentMinigameCount = 0;
    private GameObject activeMinigame;

    [Header("Lose Penalty")]
    [SerializeField] private float anxietyIncreaseOnLose = 10f;

    [Header("Normal Finish Scene")]
    [Tooltip("一般情況下完成小遊戲後要回去的 scene")]
    [SerializeField] private string defaultNextSceneName = "2_atHomeAfterWakeUp";

    [Header("Special Day Scene Rules")]
    [Tooltip("指定某些天數改跳特殊動畫 scene")]
    [SerializeField] private List<SpecialDaySceneRule> specialDaySceneRules = new List<SpecialDaySceneRule>();

    [Header("Advance Day")]
    [Tooltip("全部完成後是否先推進一天")]
    [SerializeField] private bool advanceDayOnFinish = true;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [Header("Available Minigames")]
    public List<MinigameOption> minigameOptions = new List<MinigameOption>();

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => newGameManager.Instance != null && newGameManager.Instance.playerStats != null);
        Debug.Log($"🧠 newGameManager found! Anxiety = {newGameManager.Instance.playerStats.anxiety}");
        StartMinigameSequence();
    }

    public void StartMinigameSequence()
    {
        currentMinigameCount = 0;
        StartCoroutine(PlayMinigameFlow());
    }

    private IEnumerator PlayMinigameFlow()
    {
        Debug.Log("▶️ Starting PlayMinigameFlow()");
        currentMinigameCount = 0;

        while (currentMinigameCount < minigamesToPlay)
        {
            Debug.Log($"--- Round {currentMinigameCount + 1} ---");

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

            if (option.introVideo != null)
                yield return PlayCutscene(option.introVideo);

            activeMinigame = Instantiate(option.minigamePrefab);

            MinigameBase minigame = activeMinigame.GetComponentInChildren<MinigameBase>(true);
            if (minigame == null)
            {
                Debug.LogError($"❌ The prefab '{option.minigamePrefab.name}' has NO MinigameBase.");
                Destroy(activeMinigame);
                activeMinigame = null;
                yield break;
            }

            TimedMinigameBase timed = activeMinigame.GetComponentInChildren<TimedMinigameBase>(true);
            if (timed != null)
            {
                timed.SetTimeLimit(option.timeLimitSeconds);
            }

            bool? result = null;
            minigame.OnMinigameEnd += (bool won) => { result = won; };

            yield return new WaitUntil(() => result.HasValue);

            Debug.Log($"🏁 Minigame finished, result = {result.Value}");

            if (!result.Value)
            {
                ApplyLosePenalty();
            }

            if (result.Value && option.winVideo != null)
                yield return PlayCutscene(option.winVideo);
            else if (!result.Value && option.loseVideo != null)
                yield return PlayCutscene(option.loseVideo);

            Destroy(activeMinigame);
            activeMinigame = null;

            currentMinigameCount++;
        }

        FinishAllMinigames();
    }

    private void ApplyLosePenalty()
    {
        if (newGameManager.Instance == null || newGameManager.Instance.playerStats == null)
        {
            Debug.LogWarning("⚠ newGameManager or playerStats is null, cannot add anxiety on lose.");
            return;
        }

        newGameManager.Instance.playerStats.UpdateAnxiety(anxietyIncreaseOnLose);
        Debug.Log($"😰 Lost minigame. Anxiety +{anxietyIncreaseOnLose}");
    }

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

        if (raw != null)
            raw.enabled = false;
    }

    private void FinishAllMinigames()
    {
        Debug.Log($"✅ All {minigamesToPlay} minigames complete!");

        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(-10f);
        }

        if (advanceDayOnFinish)
        {
            TryAdvanceDay();
        }

        string targetScene = GetTargetSceneForCurrentDay();

        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"📦 Loading target scene: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("⚠ 沒有可載入的場景名稱。");
        }
    }

    private string GetTargetSceneForCurrentDay()
    {
        int currentDay = GetCurrentDay();

        foreach (var rule in specialDaySceneRules)
        {
            if (rule.day == currentDay && !string.IsNullOrEmpty(rule.sceneName))
            {
                Debug.Log($"🎬 Day {currentDay} matches special rule, loading special scene: {rule.sceneName}");
                return rule.sceneName;
            }
        }

        Debug.Log($"🏠 Day {currentDay} uses default scene: {defaultNextSceneName}");
        return defaultNextSceneName;
    }

    private int GetCurrentDay()
    {
        if (newGameManager.Instance == null || newGameManager.Instance.timeSystem == null)
        {
            Debug.LogWarning("⚠ newGameManager or timeSystem is null, fallback day = 1");
            return 1;
        }

        object timeSystemObj = newGameManager.Instance.timeSystem;
        System.Type type = timeSystemObj.GetType();

        var dayField = type.GetField("day");
        if (dayField != null && dayField.FieldType == typeof(int))
        {
            return (int)dayField.GetValue(timeSystemObj);
        }

        var currentDayField = type.GetField("currentDay");
        if (currentDayField != null && currentDayField.FieldType == typeof(int))
        {
            return (int)currentDayField.GetValue(timeSystemObj);
        }

        var dayProperty = type.GetProperty("day");
        if (dayProperty != null && dayProperty.PropertyType == typeof(int))
        {
            return (int)dayProperty.GetValue(timeSystemObj);
        }

        var currentDayProperty = type.GetProperty("currentDay");
        if (currentDayProperty != null && currentDayProperty.PropertyType == typeof(int))
        {
            return (int)currentDayProperty.GetValue(timeSystemObj);
        }

        Debug.LogWarning("⚠ 找不到 day / currentDay，fallback day = 1");
        return 1;
    }

    private void TryAdvanceDay()
    {
        if (newGameManager.Instance == null)
        {
            Debug.LogWarning("⚠ newGameManager.Instance is null, cannot advance day.");
            return;
        }

        object timeSystemObj = newGameManager.Instance.timeSystem;
        if (timeSystemObj == null)
        {
            Debug.LogWarning("⚠ timeSystem is null, cannot advance day.");
            return;
        }

        System.Type type = timeSystemObj.GetType();

        var addDayMethod = type.GetMethod("AddDay");
        if (addDayMethod != null)
        {
            var parameters = addDayMethod.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
            {
                addDayMethod.Invoke(timeSystemObj, new object[] { 1 });
                Debug.Log("📅 Day +1 via AddDay(1)");
                return;
            }
        }

        var nextDayMethod = type.GetMethod("NextDay");
        if (nextDayMethod != null && nextDayMethod.GetParameters().Length == 0)
        {
            nextDayMethod.Invoke(timeSystemObj, null);
            Debug.Log("📅 Day +1 via NextDay()");
            return;
        }

        var dayField = type.GetField("day");
        if (dayField != null && dayField.FieldType == typeof(int))
        {
            int current = (int)dayField.GetValue(timeSystemObj);
            dayField.SetValue(timeSystemObj, current + 1);
            Debug.Log($"📅 Day field increased: {current} -> {current + 1}");
            return;
        }

        var currentDayField = type.GetField("currentDay");
        if (currentDayField != null && currentDayField.FieldType == typeof(int))
        {
            int current = (int)currentDayField.GetValue(timeSystemObj);
            currentDayField.SetValue(timeSystemObj, current + 1);
            Debug.Log($"📅 currentDay field increased: {current} -> {current + 1}");
            return;
        }

        Debug.LogWarning("⚠ 找不到可用的日期推進方法。");
    }

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
            if (anxiety >= option.minAnxiety && anxiety < option.maxAnxiety)
                validOptions.Add(option);
        }

        if (validOptions.Count == 0)
        {
            Debug.LogWarning($"⚠ No minigame matches anxiety level {anxiety}. Defaulting to first option.");
            return minigameOptions.Count > 0 ? minigameOptions[0] : null;
        }

        int index = Random.Range(0, validOptions.Count);
        return validOptions[index];
    }
}   