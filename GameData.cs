using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;
    
    public int highScore = 0;
    public int lastScore = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetScore(int score)
    {
        lastScore = score;
        if (score > highScore)
        {
            highScore = score;
        }
    }
}