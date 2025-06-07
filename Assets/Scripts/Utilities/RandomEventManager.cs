using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class RandomEventManager: MonoBehaviour
{
    private List<string> triggeredEvents = new List<string>();
    // private newGameManager.Instance newGameManager.Instance;
    // private newGameManager.Instance.TimeSystem newGameManager.Instance.timeSystem;
    public TimeSystem timeSystem;
    public GameObject eventPanelPrefab; // 在 Inspector 拖入
    private GameObject currentEventPanel;


    // 觸發隨機事件（基於隨機碼）
    public void TriggerRandomEvent()
    {
        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";
        if (randomCode <= 30) // 低難度
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(5f);
            eventName = "LowDifficultyEvent";
        }
        else if (randomCode <= 60) // 中難度
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(10f);
            eventName = "MediumDifficultyEvent";
        }
        else if (randomCode <= 90) // 高難度
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(20f);
            eventName = "HighDifficultyEvent";
        }
        else // 特殊事件
        {
            eventName = "SpecialEvent";
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        // 事件完成後推進時間（1小時）
        newGameManager.Instance.timeSystem.AddEventTime(1f);

        // 觸發下一個隨機事件
        // newnewGameManager.Instance.Instance.TriggerNextEvent();
    }

    public void TriggerRandomEvent_home()
    {
        int randomCode = UnityEngine.Random.Range(0, 99);
        string eventName = "";
        switch (randomCode)
        {
            case 0:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                // 事件完成後推進時間（1小時）
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            case 1:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            default:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
        Debug.Log(newGameManager.Instance == null);
        Debug.Log(newGameManager.Instance.timeUI == null);
        Debug.Log(newGameManager.Instance.timeSystem == null);
        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        
        

        // 觸發下一個隨機事件
        // newnewGameManager.Instance.Instance.TriggerNextEvent();
    }

    public void TriggerRandomEvent_shop()
    {
        int randomCode = UnityEngine.Random.Range(100, 199);
        string eventName = "";
        switch (randomCode)
        {
            case 0:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                // 事件完成後推進時間（1小時）
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            case 1:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            default:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
        }
        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        
    }

    public void TriggerRandomEvent_outside()
    {
        int randomCode = UnityEngine.Random.Range(200, 299);
        string eventName = "";
        switch (randomCode)
        {
            case 0:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                // 事件完成後推進時間（1小時）
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            case 1:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            default:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
        }
        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_sleep()
    {
        int randomCode = UnityEngine.Random.Range(100, 199);
        string eventName = "";
        eventName = "SleepEvent";
        // 事件完成後推進時間（1小時）
        newGameManager.Instance.timeSystem.AddEventTime(5f);
        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
        Debug.Log(newGameManager.Instance == null);
        Debug.Log(newGameManager.Instance.timeUI == null);
        Debug.Log(newGameManager.Instance.timeSystem == null);
        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);           
    }

    public void TriggerRandomEvent_milk()
    {
        string eventName = "";
        eventName = "BuyMilk";
        // 事件完成後推進時間（1小時）
        TriggerEvent(eventName, true);
        Debug.Log($"觸發事件：{eventName}");
        Debug.Log(newGameManager.Instance == null);
        Debug.Log(newGameManager.Instance.timeUI == null);
        Debug.Log(newGameManager.Instance.timeSystem == null);
    }

    public void atHomeEvent()
    {
        // TimeSystem.goOut = false;
        newGameManager.Instance.playerStats.nextAction = NextAction.goBackHome;
        // SceneManage.LoadScene("5_atHome");
        newGameManager.Instance.OnTimeManuallyUpdated();
        newGameManager.Instance.playerStats.nextAction = NextAction.none;
    }

    public void toSchoolEvent()
    {
        // TimeSystem.goOut = true;
        newGameManager.Instance.playerStats.nextAction = NextAction.goOut;
        newGameManager.Instance.timeSystem.AddEventTime(1f);
        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        newGameManager.Instance.playerStats.nextAction = NextAction.none;
        // SceneManage.LoadScene("4_atSchool");
    }

    // 觸發固定事件（由newGameManager.Instance.TimeSystem調用）
    public void TriggerEvent(string eventName, bool isMandatory)
    {
        if (!isMandatory) return;

        // 避免重複觸發（對於強制事件，需檢查是否已觸發）
        if (triggeredEvents.Contains(eventName) && eventName != "BuyMilk") return; // BuyMilk可能重複觸發

        triggeredEvents.Add(eventName);

        switch (eventName)
        {
            case "ReceiveMessage":
                Debug.Log("收到訊息！");
                newGameManager.Instance.playerStats.UpdateAnxiety(2f);
                FindObjectOfType<gameUIManager>().ShowMessage("xxxxxxxxxxxx");
                //newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
            case "BuyMilk":
                Debug.Log("去超市買牛奶！");
                Inventory.PickupItem("Milk");
                //SceneManage.SwitchScene(TimePeriod.AtSupermarket);
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                //ShowEventPanel("媽媽請你去買牛奶。你在超市找了半天才找到。");
                FindObjectOfType<gameUIManager>().ShowMilk();
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
                // TimeSystem.goToMarket = false;
                break;
            case "ReceiveCatVideo":
                Debug.Log("收到貓咪影片！");
                newGameManager.Instance.playerStats.UpdateAnxiety(-5f);
                ShowEventPanel("你收到了超可愛的貓咪影片，心情稍微放鬆了一點。");
                break;
            case "ParentsArgue":
                Debug.Log("爸媽吵架了...");
                newGameManager.Instance.playerStats.UpdateAnxiety(10f); // 增加焦慮
                // SceneManage.SwitchScene(TimePeriod.AtHomeParentsArgue); // 切換到吵架場景
                break;
            case "MeetSido":
                Debug.Log("探險中遇到希多！");
                newGameManager.Instance.playerStats.UpdateAnxiety(8f); // 增加焦慮
                // SceneManage.SwitchScene(TimePeriod.AtAdventure); // 切換到探險場景
                break;
            case "LowPopularityEvent":
                Debug.Log("校園熱門度太低，觸發事件！");
                // newGameManager.Instance.playerStats.UpdateAnxiety(15f); // 增加焦慮
                // SceneManage.SwitchScene(TimePeriod.AtSchoolAfterClass); // 切換到放學場景
                break;
        }

        // 觸發下一個隨機事件
        // newGameManager.Instance.TriggerNextEvent();
        newGameManager.Instance.OnTimeManuallyUpdated();
    }

    private void ShowEventPanel(string description)
    {
        if (currentEventPanel != null) Destroy(currentEventPanel);

        currentEventPanel = Instantiate(eventPanelPrefab, GameObject.Find("Canvas").transform);
        currentEventPanel.transform.Find("DescriptionText").GetComponent<TMPro.TextMeshProUGUI>().text = description;

        Button closeButton = currentEventPanel.transform.Find("CloseButton").GetComponent<Button>();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(currentEventPanel);
        });
    }

    // 檢查事件是否已觸發
    public bool HasTriggered(string eventName)
    {
        return triggeredEvents.Contains(eventName);
    }

    public List<string> GetTriggeredEvents()
    {
        return triggeredEvents;
    }
}