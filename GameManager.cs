using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
    public GameObject ghostPrefab;
    public int maxGhosts = 5;
    public float spawnRange = 30f;

    [Header("Game State")]
    public int score = 0;
    public int ghostsCaught = 0;
    private bool gameActive = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        score = 0;
        ghostsCaught = 0;
        gameActive = true;
        
        Invoke("SpawnAllGhosts", 1f);
    }

    void SpawnAllGhosts()
    {
        for (int i = 0; i < maxGhosts; i++)
        {
            SpawnGhost();
        }
    }

    void SpawnGhost()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            1f,
            Random.Range(-spawnRange, spawnRange)
        );
        Instantiate(ghostPrefab, randomPos, Quaternion.identity);
    }

    public void OnGhostCaught()
    {
        if (!gameActive) return;

        ghostsCaught++;
        score++;

        Debug.Log($"Призрак пойман! Счет: {score}/{maxGhosts}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }

        if (ghostsCaught >= maxGhosts)
        {
            Victory();
        }
    }

    void Victory()
    {
        gameActive = false;
        Debug.Log("Победа! Все призраки пойманы.");
        
        if (GameData.Instance != null)
        {
            GameData.Instance.SetScore(score);
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }
    }
}