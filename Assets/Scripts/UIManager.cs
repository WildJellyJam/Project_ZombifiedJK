using UnityEngine;
using UnityEngine.UI;
using TMPro; // 使用TextMeshPro（Unity建議的文字組件）
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 主頁面UI元素
    [Header("主頁面UI")]
    public Button continueButton;     // 繼續遊戲按鈕
    public Button newGameButton;      // 開始新遊戲按鈕
    public Button exitButton;         // 退出按鈕
    public GameObject saveListPanel;  // 存檔列表面板
    public Button[] saveSlotButtons;  // 存檔槽按鈕（3個）

    // 遊戲內UI元素
    [Header("遊戲內UI")]
    public GameObject statsPanel;     // 數值顯示面板
    public Slider sanitySlider;       // 理智值進度條
    public Slider socialEnergySlider; // 社交能量進度條
    public Slider popularitySlider;   // 校園熱門度進度條
    public Slider anxietySlider;      // 焦慮指數進度條
    public TextMeshProUGUI affectionText; // 好感值顯示

    // 劇情選擇UI
    [Header("劇情選擇UI")]
    public GameObject choicePanel;    // 選擇面板
    public Button[] choiceButtons;    // 選擇按鈕（最多3個）

    // 結局UI
    [Header("結局UI")]
    public GameObject endingPanel;    // 結局面板
    public TextMeshProUGUI endingText;// 結局文字
    public Button replayButton;       // 重玩按鈕
    public Button returnToMenuButton; // 回到主頁面按鈕

    private GameManager gameManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 獲取GameManager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("未找到GameManager，請確保場景中有GameManager物件！");
        }

        // 綁定主頁面按鈕事件
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        // 綁定存檔槽按鈕
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i;
            saveSlotButtons[i].onClick.AddListener(() => OnSaveSlotClicked(slotIndex));
        }

        // 綁定結局按鈕
        replayButton.onClick.AddListener(OnReplayButtonClicked);
        returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);

        // 初始化UI
        saveListPanel.SetActive(false);
        statsPanel.SetActive(false);
        choicePanel.SetActive(false);
        endingPanel.SetActive(false);
    }

    void Update()
    {
        // 動態更新數值顯示
        if (statsPanel.activeSelf)
        {
            UpdateStatsUI();
        }
    }

    // 主頁面按鈕事件
    private void OnContinueButtonClicked()
    {
        saveListPanel.SetActive(true); // 顯示存檔列表
        UpdateSaveListUI();
    }

    private void OnNewGameButtonClicked()
    {
        gameManager.StartNewGame();
        statsPanel.SetActive(true); // 進入遊戲後顯示數值
    }

    private void OnExitButtonClicked()
    {
        Application.Quit();
    }

    // 存檔槽按鈕事件
    private void OnSaveSlotClicked(int slotIndex)
    {
        gameManager.LoadGame(slotIndex);
        saveListPanel.SetActive(false);
        statsPanel.SetActive(true); // 進入遊戲後顯示數值
    }

    // 更新存檔列表UI
    private void UpdateSaveListUI()
    {
        var saveSlots = gameManager.saveSystem.GetSaveSlots();
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            if (i < saveSlots.Count && saveSlots[i] != null)
            {
                saveSlotButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = 
                    $"存檔 {i + 1}: {saveSlots[i].timestamp}, Day {saveSlots[i].gameTime.day}";
            }
            else
            {
                saveSlotButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"存檔 {i + 1}: 空";
            }
        }
    }

    // 更新數值顯示
    private void UpdateStatsUI()
    {
        sanitySlider.value = gameManager.playerStats.sanity / 100f;
        socialEnergySlider.value = gameManager.playerStats.socialEnergy / 100f;
        popularitySlider.value = gameManager.playerStats.popularity / 100f;
        anxietySlider.value = gameManager.playerStats.anxiety / 120f;

        // 顯示好感值（假設當前與某NPC互動）
        string affectionDisplay = "";
        foreach (var npc in gameManager.npcAffection.affection)
        {
            affectionDisplay += $"{npc.Key}: {npc.Value}\n";
        }
        affectionText.text = affectionDisplay;
    }

    // 顯示劇情選擇
    public void ShowChoices(string[] choices, System.Action<int> onChoiceSelected)
    {
        choicePanel.SetActive(true);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(i < choices.Length);
            if (i < choices.Length)
            {
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[i];
                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => 
                {
                    onChoiceSelected(choiceIndex);
                    choicePanel.SetActive(false);
                });
            }
        }
    }

    // 顯示結局
    public void ShowEnding(string endingType)
    {
        endingPanel.SetActive(true);
        endingText.text = endingType switch
        {
            "Good" => "好結局：壓力低於20，恭喜你成功度過一周！",
            "Normal" => "普通結局：壓力適中，生活還算平穩。",
            "Bad" => "壞結局：壓力過高，你崩潰了...",
            _ => "未知結局"
        };
    }

    private void OnReplayButtonClicked()
    {
        gameManager.StartNewGame();
        endingPanel.SetActive(false);
        statsPanel.SetActive(true);
    }

    private void OnReturnToMenuButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        endingPanel.SetActive(false);
        statsPanel.SetActive(false);
    }
}