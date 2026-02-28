using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject obstaclePrefab;
    public int maxObstacles = 5;
    public float spawnRange = 10f;
    public float spawnHeight = 0f;

    void Start()
    {
        if (obstaclePrefab == null)
        {
            Debug.LogWarning("ObstacleGenerator: Не назначен obstaclePrefab!");
            return;
        }
        GenerateObstacles();
    }

    void GenerateObstacles()
    {
        for (int i = 0; i < maxObstacles; i++)
        {
            SpawnObstacle();
        }
    }

    void SpawnObstacle()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            spawnHeight,
            Random.Range(-spawnRange, spawnRange)
        );

        Instantiate(obstaclePrefab, randomPos, Quaternion.identity);
    }
}