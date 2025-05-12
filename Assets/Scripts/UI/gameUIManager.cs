
using UnityEngine;
using UnityEngine.UI;
public class gameUIManager : MonoBehaviour
{

    public GameObject pausePanel;
    public Button returnToMenuButton;

    
    public bool isPaused = false;
    
    void OnEnable()
    {
        newGameManager.Instance.ReturnToMainMenuEvent += ReturnToMainMenu;
    }
    void OnDisable()
    {
        newGameManager.Instance.ReturnToMainMenuEvent -= ReturnToMainMenu;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);
        else
        {
            Debug.LogError("continueButton 未找到，請檢查 MainMenuButtonsContainer 中是否存在 'continueBtn'！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausePanel();
        }
    }

    private void ReturnToMainMenu()
    {
        TogglePausePanel();
        isPaused = false;
        pausePanel.SetActive(false);
    }

    private void TogglePausePanel()
    {
        isPaused = !isPaused;
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
            Debug.Log(isPaused ? "顯示暫停選單" : "隱藏暫停選單");
        }
    }

    private void OnReturnToMenuButtonClicked()
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene("StartUpMenu");
        ReturnToMainMenu();
    }
}
