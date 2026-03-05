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
    
    // ✅ PTS: Статистика пакетов
    private int ptsSent = 0;
    private int ptsReceived = 0;

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
        
        // ✅ ПРОВЕРКА: Есть ли PhotonView на GameManager
        PhotonView gmPv = GetComponent<PhotonView>();
        if (gmPv == null)
        {
            Debug.LogError("❌ ОШИБКА: Нет PhotonView на GameManager! Добавьте компонент!");
        }
        else
        {
            Debug.Log($"✅ PhotonView найден на GameManager: View ID = {gmPv.ViewID}");
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
            
            // ✅ PTS: Запуск мониторинга пакетов
            InvokeRepeating("MonitorPTS", 5f, 5f);
        }
    }

    // ✅ PTS: Мониторинг статистики пакетов
    void MonitorPTS()
    {
        Debug.Log($"📊 PTS STAT: Отправлено={ptsSent} | Получено={ptsReceived}");
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
        if (ghostPrefab == null)
        {
            Debug.LogError("❌ ОШИБКА: ghostPrefab не назначен в GameManager!");
            return;
        }
        
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
        
        PhotonNetwork.Instantiate(ghostPrefab.name, randomPos, Quaternion.identity, 0);
        
        ptsSent++;
        Debug.Log($"👻 Призрак создан: {ghostPrefab.name} в позиции {randomPos}");
    }

    // ✅ PTS: СЕТЕВОЙ ВЫЗОВ с данными пакета
    [PunRPC]
    public void OnGhostCaughtRPC(int newScore, int newGhostsCaught, float timestamp)
    {
        Debug.Log($"📦 PTS RPC: Призрак пойман! Счёт={newScore} | Время={timestamp}");
        
        if (!gameActive) return;
        
        score = newScore;
        ghostsCaught = newGhostsCaught;
        ptsReceived++;
        
        Debug.Log($"Призрак пойман! Счет: {score}/{maxGhosts}");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(score);
        }
        
        if (ghostsCaught >= maxGhosts)
        {
            VictoryRPC(Time.time);
        }
    }

    // ✅ PTS: Публичный метод с формированием пакета
    public void OnGhostCaught()
    {
        // ✅ ПРОВЕРКА: Есть ли подключение
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("⚠️ OnGhostCaught: Нет подключения к сети!");
            OnGhostCaughtRPC(score + 1, ghostsCaught + 1, Time.time);
            return;
        }
        
        if (photonView != null)
        {
            // ✅ ОТПРАВКА PTS ПАКЕТА всем клиентам
            photonView.RPC("OnGhostCaughtRPC", RpcTarget.All, 
                          score + 1, ghostsCaught + 1, Time.time);
        }
        else
        {
            DebugLogError("❌ ОШИБКА: photonView == null в OnGhostCaught!");
            OnGhostCaughtRPC(score + 1, ghostsCaught + 1, Time.time);
        }
    }

    // ✅ PTS: VictoryRPC с временной меткой
    [PunRPC]
    void VictoryRPC(float timestamp)
    {
        Debug.Log($"📦 PTS RPC: ПОБЕДА! Время={timestamp}");
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
            photonView.RPC("VictoryRPC", RpcTarget.All, Time.time);
        }
        else
        {
            VictoryRPC(Time.time);
        }
    }
    
    void DebugLogError(string message)
    {
        Debug.LogError($"[GameManager] {message}");
    }
}