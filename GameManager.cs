using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
            // ✅ ПРОВЕРКА ПРЕФАБА
        if (ghostPrefab == null)
        {
            Debug.LogError("❌ ОШИБКА: Ghost Prefab не назначен!");
            return;
        }
        
        Debug.Log($"✅ Ghost Prefab назначен: {ghostPrefab.name}");
        
        PhotonView pv = ghostPrefab.GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogError($"❌ ОШИБКА: Нет PhotonView на {ghostPrefab.name}!");
        }
        else
        {
            Debug.Log($"✅ PhotonView найден на {ghostPrefab.name}");
        }
    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("🎮 GameplayCursor: Курсор заблокирован");
        // Только хост инициализирует игру
        if (PhotonNetwork.IsMasterClient)
        {
            score = 0;
            ghostsCaught = 0;
            gameActive = true;
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameMusic();
            }
            
            Invoke("SpawnAllGhosts", 1f);
        }
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
        // ✅ ПРОВЕРКА: Существует ли префаб
        if (ghostPrefab == null)
        {
            Debug.LogError("❌ ОШИБКА: ghostPrefab не назначен в GameManager!");
            return;
        }
        
        // ✅ ПРОВЕРКА: Есть ли PhotonView на префабе
        PhotonView pv = ghostPrefab.GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogError($"❌ ОШИБКА: У префаба {ghostPrefab.name} нет компонента PhotonView!");
            Debug.LogError("📁 Добавьте PhotonView на префаб Ghost в Resources!");
            return;
        }
        
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            1f,
            Random.Range(-spawnRange, spawnRange)
        );
        
        // ✅ Сетевой инстанс призрака (используем имя префаба)
        PhotonNetwork.Instantiate(ghostPrefab.name, randomPos, Quaternion.identity, 0);
        
        Debug.Log($"👻 Призрак создан: {ghostPrefab.name} в позиции {randomPos}");
    }
    
    [PunRPC]
    public void OnGhostCaughtRPC()
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
            VictoryRPC();
        }
    }
    
    public void OnGhostCaught()
    {
        if (photonView != null)
        {
            photonView.RPC("OnGhostCaughtRPC", RpcTarget.All);
        }
        else
        {
            // Если нет photonView, вызываем локально
            OnGhostCaughtRPC();
        }
    }
    
    [PunRPC]
    void VictoryRPC()
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
    
    void Victory()
    {
        if (photonView != null)
        {
            photonView.RPC("VictoryRPC", RpcTarget.All);
        }
    }
}