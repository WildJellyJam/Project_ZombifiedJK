// TooltipSystem.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance;

    public GameObject tooltipObject;
    public TMP_Text tooltipText;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (tooltipObject.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipObject.transform.parent.GetComponent<RectTransform>(),
                Input.mousePosition,
                null, out pos);
            tooltipObject.transform.localPosition = pos + new Vector2(10f, -10f); // 顯示在滑鼠右下
        }
    }

    public void Show(string message)
    {
        tooltipText.text = message;
        tooltipObject.SetActive(true);
    }

    public void Hide()
    {
        tooltipObject.SetActive(false);
    }
}
