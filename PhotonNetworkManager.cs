using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance;
    
    [Header("Settings")]
    public string gameVersion = "1.0";
    public byte maxPlayers = 4;
    public string roomName = "GhostVacuumRoom";
    
    [Header("Debug")]
    public bool debugMode = true;
    
    [Header("Reconnection")]
    public int maxReconnectAttempts = 3;
    private int reconnectAttempts = 0;
    private bool isReconnecting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLog("PhotonNetworkManager: Создан и сохранён между сценами");
        }
        else
        {
            DebugLog("PhotonNetworkManager: Дубликат удалён!");
            Destroy(gameObject);
            return;
        }
        
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;
        
        PhotonNetwork.KeepAliveInBackground = 3000;
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        
        
        DebugLog($"GameVersion: {gameVersion}");
        DebugLog($"RoomName: {roomName}");
        DebugLog($"MaxPlayers: {maxPlayers}");
    }
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        DebugLog("PhotonNetworkManager.Start() вызван");
        DebugLog($"PhotonNetwork.IsConnected: {PhotonNetwork.IsConnected}");
        
        ConnectToPhoton();
    }
    
    void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
        }
    }
    
    public void ConnectToPhoton()
    {
        DebugLog("Подключение к Photon Server...");
        DebugLog($"PhotonNetwork.OfflineMode: {PhotonNetwork.OfflineMode}");
        
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        DebugLog("Подключено к Photon Master Server!");
        DebugLog($"Region: {PhotonNetwork.CloudRegion}");
        DebugLog($"Server Address: {PhotonNetwork.ServerAddress}");
        DebugLog($"IsConnectedAndReady: {PhotonNetwork.IsConnectedAndReady}");
        
        reconnectAttempts = 0;
        isReconnecting = false;
        
        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinOrCreateRoom();
        }
    }
    
    public void JoinOrCreateRoom()
    {
        DebugLog($"Поиск или создание комнаты: {roomName}");
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        roomOptions.PublishUserId = true;
        
        roomOptions.EmptyRoomTtl = 60000;
        roomOptions.PlayerTtl = 60000;
        
        DebugLog($"RoomOptions.MaxPlayers: {roomOptions.MaxPlayers}");
        DebugLog($"RoomOptions.IsVisible: {roomOptions.IsVisible}");
        
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
    
    public override void OnJoinedRoom()
    {
        DebugLog("Игрок в комнате!");
        DebugLog($"Игроков в комнате: {PhotonNetwork.CurrentRoom.PlayerCount}");
        DebugLog($"Room Name: {PhotonNetwork.CurrentRoom.Name}");
        DebugLog($"IsMasterClient: {PhotonNetwork.IsMasterClient}");
        
        SpawnPlayer();
    }
    
    public void SpawnPlayer()
    {
        string prefabName = "PlayerNetworkPrefab";
        
        GameObject prefabCheck = Resources.Load<GameObject>(prefabName);
        
        if (prefabCheck == null)
        {
            DebugLogError($"ОШИБКА: Префаб '{prefabName}' не найден в Resources!");
            DebugLogError($"Проверьте путь: Assets/Resources/{prefabName}.prefab");
            return;
        }
        
        DebugLog($"Префаб найден: {prefabCheck.name}");
        
        PhotonView pv = prefabCheck.GetComponent<PhotonView>();
        if (pv == null)
        {
            DebugLogError($"ОШИБКА: У префаба нет компонента PhotonView!");
            return;
        }
        
        DebugLog($"PhotonView найден: View ID = {pv.ViewID}");
        
        bool hasObserved = false;
        if (pv.ObservedComponents != null && pv.ObservedComponents.Count > 0)
        {
            foreach (var component in pv.ObservedComponents)
            {
                if (component != null)
                {
                    hasObserved = true;
                    DebugLog($"ObservedComponents настроен: {component.GetType().Name}");
                    break;
                }
            }
        }
        if (!hasObserved)
        {
            DebugLogWarning($"PhotonView.ObservedComponents пуст! Синхронизация может не работать.");
        }
        
        Vector3 spawnPos = new Vector3(0, 1, 0);
        PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity, 0);
        
        DebugLog("Игрок создан в сети!");
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        DebugLogWarning($"Отключено от сервера: {cause}");
        DebugLogError($"ПРИЧИНА: {cause.ToString()}");
        DebugLogError($"PhotonNetwork.IsConnected: {PhotonNetwork.IsConnected}");
        DebugLogError($"NetworkClientState: {PhotonNetwork.NetworkClientState}");
        
        if (cause == DisconnectCause.ClientTimeout || cause == DisconnectCause.Exception)
        {
            if (reconnectAttempts < maxReconnectAttempts && !isReconnecting)
            {
                reconnectAttempts++;
                isReconnecting = true;
                DebugLog($"Попытка реконнекта {reconnectAttempts}/{maxReconnectAttempts}...");
                
                Invoke(nameof(ConnectToPhoton), 3f);
            }
            else
            {
                DebugLogError($"Превышено количество попыток реконнекта. Проверьте сеть!");
            }
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        DebugLog($"Новый игрок: {newPlayer.NickName}");
        DebugLog($"Игроков в комнате: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DebugLog($"Игрок вышел: {otherPlayer.NickName}");
        DebugLog($"Игроков в комнате: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        DebugLogError($"Ошибка входа в комнату: {returnCode} - {message}");
        
        if (returnCode == 32746)
        {
            DebugLog("UserId уже в комнате. Пробуем покинуть и зайти снова...");
            PhotonNetwork.LeaveRoom(false);
            Invoke(nameof(JoinOrCreateRoom), 2f);
        }
    }
    
    public override void OnCreatedRoom()
    {
        DebugLog("Комната создана!");
        DebugLog($"Room Name: {PhotonNetwork.CurrentRoom.Name}");
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        DebugLogError($"Ошибка создания комнаты: {returnCode} - {message}");
    }
    
    void DebugLog(string message)
    {
        if (debugMode) Debug.Log($"[PhotonNetworkManager] {message}");
    }
    
    void DebugLogWarning(string message)
    {
        if (debugMode) Debug.LogWarning($"[PhotonNetworkManager] {message}");
    }
    
    void DebugLogError(string message)
    {
        if (debugMode) Debug.LogError($"[PhotonNetworkManager] {message}");
    }
}