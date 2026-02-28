using UnityEngine;


public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject ghostPrefab;
    public int maxGhosts = 5;
    public float spawnRange = 10f;

    void Start()
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
}