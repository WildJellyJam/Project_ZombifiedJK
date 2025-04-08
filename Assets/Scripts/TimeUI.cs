using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    void Start()
    {
        GameManager.OnTimeUpdated += UpdateTimeDisplay;
    }

    void OnDestroy()
    {
        GameManager.OnTimeUpdated -= UpdateTimeDisplay;
    }

    void UpdateTimeDisplay(float time)
    {
        int hours = (int)time;
        int minutes = (int)((time - hours) * 60);
        timeText.text = $"Time: {hours:00}:{minutes:00}";
    }
}