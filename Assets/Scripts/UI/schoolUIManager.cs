
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class schoolUIManager : MonoBehaviour
{

    public GameObject pausePanel;
    public Button returnToMenuButton;


    public bool isPaused = false;
    public TextMeshProUGUI messageText; // 顯示的訊息內容
    public RectTransform messageBox;
    public float showDuration = 3f; // 顯示時間（秒）
    public float slideSpeed = 500f; // 移動速度（越大越快）

    private Vector2 hiddenPos; // 螢幕外的位置
    private Vector2 visiblePos; // 顯示的位置
    private Coroutine slideCoroutine;
    private CameraManager cameraManager;

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
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager == null) Debug.Log("not catch camera manager");
        Debug.Log(cameraManager.usingFixedCamera);
        hiddenPos = new Vector2(-150, 100);    // 螢幕外上方
        visiblePos = new Vector2(-150, 10);   // 螢幕內顯示位置
        if(messageBox != null) messageBox.anchoredPosition = hiddenPos;
        pausePanel.gameObject.SetActive(false); // 預設關閉
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausePanel();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            newGameManager.Instance.timeSystem.AddEventTime(1f);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            cameraManager.ToggleCamera();
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
        newGameManager.Instance.ReturnToMainMenu_gm();

    }

    // public void ShowMessage(string msg)
    // {
    //     if (slideCoroutine != null)
    //         StopCoroutine(slideCoroutine);

    //     panel.gameObject.SetActive(true);
    //     messageText.text = msg;
    //     slideCoroutine = StartCoroutine(SlideInOut());
    // } 

    // private IEnumerator SlideInOut()
    // {
    //     // 滑入
    //     yield return StartCoroutine(SlideTo(visiblePos));
    //     yield return new WaitForSeconds(showDuration);
    //     // 滑出
    //     yield return StartCoroutine(SlideTo(hiddenPos));
    //     panel.gameObject.SetActive(false);
    // }

    // private IEnumerator SlideTo(Vector2 targetPos)
    // {
    //     while (Vector2.Distance(messageBox.anchoredPosition, targetPos) > 1f)
    //     {
    //         messageBox.anchoredPosition = Vector2.MoveTowards(
    //             messageBox.anchoredPosition,
    //             targetPos,
    //             slideSpeed * Time.deltaTime
    //         );
    //         yield return null;
    //     }
    //     messageBox.anchoredPosition = targetPos;
    // }

    // public void ShowMilk()
    // {
    //     Debug.Log("進來ㄌ！");
    //     shopBuyMilkPanel.gameObject.SetActive(true);
    //     // TimeSystem.goToMarket = true;
    //     newGameManager.Instance.playerStats.nextAction = NextAction.goToMarket;
    // }
}
