using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSceneSpawner : MonoBehaviour
{
    [Header("Character Settings")]
    public GameObject characterPrefab;     // Prefab with SpriteRenderer
    public string[] scenesToShowCharacter; // Scene names where she appears

    private GameObject spawnedCharacter;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldShow = false;

        // Check if this scene is in the list
        foreach (string sceneName in scenesToShowCharacter)
        {
            if (scene.name == sceneName)
            {
                shouldShow = true;
                break;
            }
        }

        if (shouldShow)
        {
            SpawnCharacterInScene();
        }
        else
        {
            if (spawnedCharacter != null)
                Destroy(spawnedCharacter);
        }
    }

    private void SpawnCharacterInScene()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("❌ characterPrefab is missing!");
            return;
        }

        // spawn character into world space
        spawnedCharacter = Instantiate(characterPrefab);

        // Make sure she stays visible in every scene
        DontDestroyOnLoad(spawnedCharacter);
    }
}
