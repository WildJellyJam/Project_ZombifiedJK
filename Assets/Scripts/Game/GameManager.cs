using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int anxiety = 0;
    public bool badEndingTriggered = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddAnxiety(int amount)
    {
        anxiety += amount;
    }

    public void ReduceAnxiety(int amount)
    {
        anxiety -= amount;
    }

    public void TriggerBadEnding()
    {
        badEndingTriggered = true;
        Debug.Log("Bad Ending triggered!");
        // You could load a bad ending scene here
    }
}