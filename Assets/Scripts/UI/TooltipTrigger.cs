using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Debug Info")]
    public string itemName;

    [Header("Tooltip Content")]
    [TextArea(2, 5)]
    public string message;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipSystem.Instance != null)
        {
            TooltipSystem.Instance.Show(message);
        }
        else
        {
            Debug.LogError(
                $"[TooltipTrigger] Show 失敗：TooltipSystem.Instance 是 null\n" +
                $"物品名稱：{itemName}\n" +
                $"物件名稱：{gameObject.name}\n" +
                $"Hierarchy 路徑：{GetHierarchyPath()}\n" +
                $"Tooltip 內容：{message}",
                this
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipSystem.Instance != null)
        {
            TooltipSystem.Instance.Hide();
        }
        else
        {
            Debug.LogError(
                $"[TooltipTrigger] Hide 失敗：TooltipSystem.Instance 是 null\n" +
                $"物品名稱：{itemName}\n" +
                $"物件名稱：{gameObject.name}\n" +
                $"Hierarchy 路徑：{GetHierarchyPath()}\n" +
                $"Tooltip 內容：{message}",
                this
            );
        }
    }

    private string GetHierarchyPath()
    {
        string path = gameObject.name;
        Transform current = transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}