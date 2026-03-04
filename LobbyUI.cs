using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI connectionStatusText;
    public TextMeshProUGUI playerCountText;
    public Button startGameButton;
    
    void Start()
    {
        // ✅ ПОКАЗЫВАЕМ КУРСОР В ЛОББИ
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (startGameButton != null)
        {
            startGameButton.interactable = false;
            startGameButton.onClick.AddListener(StartGame);
        }
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        // ✅ БЕЗОПАСНАЯ ПРОВЕРКА: сначала проверяем, что текст не null
        if (connectionStatusText != null)
        {
            if (PhotonNetwork.IsConnected)
            {
                connectionStatusText.text = "Статус: " + PhotonNetwork.NetworkClientState;
            }
            else
            {
                connectionStatusText.text = "Статус: Отключено";
            }
        }
        
        // ✅ БЕЗОПАСНАЯ ПРОВЕРКА для playerCountText
        if (playerCountText != null)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.CurrentRoom != null)
            {
                playerCountText.text = "Игроков в комнате: " + PhotonNetwork.CurrentRoom.PlayerCount;
            }
            else
            {
                playerCountText.text = "Игроков в комнате: 0";
            }
        }
        
        if (startGameButton != null)
        {
            startGameButton.interactable = PhotonNetwork.IsMasterClient && 
                                          PhotonNetwork.IsConnectedAndReady;
        }
    }
    
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("🎮 Мастер запускает игру...");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}