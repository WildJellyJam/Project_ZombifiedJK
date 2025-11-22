using UnityEngine;

public class AnxietyDebugInput : MonoBehaviour
{
    private static AnxietyDebugInput _instance;

    public float step = 10f;

    private void Awake()
    {
        // Singleton pattern so it only exists once
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);   // 👈 stays across scene loads
    }

    private void Update()
    {
        if (newGameManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(+step);
            Debug.Log($"Anxiety +{step} -> {newGameManager.Instance.playerStats.anxiety}");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(-step);
            Debug.Log($"Anxiety -{step} -> {newGameManager.Instance.playerStats.anxiety}");
        }
    }
}
