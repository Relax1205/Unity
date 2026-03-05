using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject lobbyPanel;
    public TextMeshProUGUI connectionStatusText;
    public TextMeshProUGUI playerCountText;
    public Button startGameButton;

    void Start()
    {
        SetCursorForLobby();
        
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
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu" || currentScene == "LobbyScene")
        {
            SetCursorForLobby();
        }
    }

    void SetCursorForLobby()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UpdateUI()
    {
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
            Debug.Log("Мастер запускает игру...");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}