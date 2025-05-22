using UnityEngine;

public class CloseOnClick : MonoBehaviour
{
    public GameObject panelToClose;

    public void ClosePanel()
    {
        panelToClose.SetActive(false);
    }
}
