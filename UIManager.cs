using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("HUD Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;
    
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject pausePanel;
    
    [Header("Texts")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    
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
        SetupHealthUIPosition();
        SetupScoreUIPosition();
        
        Debug.Log("📋 UIManager Start: Scene = " + SceneManager.GetActiveScene().name);
        Debug.Log("🔊 UIManager: AudioManager.Instance = " + (AudioManager.Instance != null ? "✅ OK" : "❌ NULL"));
        
        if (mainMenuPanel != null && SceneManager.GetActiveScene().name == "MainMenu")
        {
            ShowMainMenu();
        }
        else if (SceneManager.GetActiveScene().name == "GameScene")
        {
            HideAllPanels();
        }
        else
        {
            HideAllPanels();
        }
    }
    
    void SetupHealthUIPosition()
    {
        if (healthSlider != null)
        {
            RectTransform rect = healthSlider.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(5, -20);
            rect.sizeDelta = new Vector2(200, 20);
        }
        
        if (healthText != null)
        {
            RectTransform rect = healthText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(5, -5);
            rect.sizeDelta = new Vector2(200, 30);
        }
    }
    
    void SetupScoreUIPosition()
    {
        if (scoreText != null)
        {
            RectTransform rect = scoreText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(110, -5);
            rect.sizeDelta = new Vector2(300, 30);
        }
    }
    
    public void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        
        if (healthText != null)
            healthText.text = current + "/" + max;
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Призраков поймано: " + score;
    }
    
    public void ShowMainMenu()
    {
        Debug.Log("📋 UIManager: ShowMainMenu called");
        Time.timeScale = 1f;
        HideAllPanels();
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
        // 🎵 ИСПРАВЛЕНИЕ: Включаем музыку меню
        Debug.Log("🔊 UIManager: AudioManager.Instance = " + (AudioManager.Instance != null ? "✅ OK" : "❌ NULL"));
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
        else
        {
            Debug.LogError("❌ UIManager: AudioManager не найден!");
        }
        
        if (highScoreText != null && GameData.Instance != null)
        {
            highScoreText.text = "Рекорд: " + GameData.Instance.highScore;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void StartGame()
    {
        // ЗВУК: Клик кнопки
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
        Debug.Log("🎮 UIManager: StartGame called");
        Time.timeScale = 1f;
        HideAllPanels();
        
        // ✅ БЛОКИРУЕМ КУРСОР ПРИ СТАРТЕ ИГРЫ
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 🎵 Музыка игры запустится в GameManager.Start() после загрузки сцены
        SceneManager.LoadScene("GameScene");
    }
    
    public void ShowGameOver()
    {
        Debug.Log("💀 UIManager: ShowGameOver called");
        Time.timeScale = 0f;
        HideAllPanels();
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = "Счет: " + GameManager.Instance.score;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ShowVictory()
    {
        Debug.Log("🏆 UIManager: ShowVictory called");
        Time.timeScale = 0f;
        HideAllPanels();
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        
        if (finalScoreText != null && GameManager.Instance != null)
        {
            finalScoreText.text = "Счет: " + GameManager.Instance.score;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void TogglePause()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
        
        if (pausePanel != null && pausePanel.activeSelf)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void PauseGame()
    {
        Debug.Log("⏸️ UIManager: PauseGame called");
        Time.timeScale = 0f;
        
        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        Debug.Log("▶️ UIManager: ResumeGame called");
        Time.timeScale = 1f;
        
        if (pausePanel != null) pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void RestartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1f;
        
        // 🎵 ИСПРАВЛЕНИЕ: Останавливаем музыку игры перед загрузкой меню
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllMusic();
        }
        
        SceneManager.LoadScene("MainMenu");
    }
    
    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
        
        #if UNITY_EDITOR
        Debug.Log("🚪 QuitGame: В редакторе игра остановлена");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }
}