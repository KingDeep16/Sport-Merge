using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton pattern for easy access
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI pauseScoreText;
    public TextMeshProUGUI pauseHighScoreText;
    public TextMeshProUGUI GOScoreText;
    public TextMeshProUGUI GOHighScoreText;
    public GameObject gameUIPanel;
    private bool _isGameOver = false;
    public GameObject gameplayContainer;
    public GameObject ballContainer;
    public GameObject ballTemplatePrefab;
    public int score = 0;
    public int highScore;
    public bool isPaused = false;

    public TextMeshProUGUI scoreText;

    [SerializeField] public List<BallData> spawnableTiers;

    [Title("Main Menu")]
    public UnityEngine.UI.Button resumeButton;
      
        [SerializeField] private float saveInterval = 5f;
private float _saveTimer;

    void Start()
    {
        LoadAudioSettings();

        UpdateScoreUI(0);
        if (PlayerPrefs.GetInt("HighScore", 0) != null)
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        
    }

    void Update()
    {
        // Check if a "SavedGame" key exists in PlayerPrefs
        resumeButton.interactable = PlayerPrefs.HasKey("SavedBallCount");
        _saveTimer += Time.deltaTime;

        if (_saveTimer >= saveInterval)
        {
            if (spawnableTiers != null && spawnableTiers.Count > 0)
            {
                SaveGameState();
                _saveTimer = 0;
            }
        }
    }

    public void AddScore(int pointsToAdd)
    {
        score += pointsToAdd;
        UpdateScoreUI(score);

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // FREEZES all physics and movement

        // Update the UI text right before showing the panel
        pauseScoreText.text = score.ToString();
        pauseHighScoreText.text = highScore.ToString();

        pausePanel.SetActive(true);
    }

    public void UnpauseGame()
    {
        isPaused = false;
       StartCoroutine(ResumeTimeWithDelay());
        pausePanel.SetActive(false);
    }


void Awake()
    {
        // Simple Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Time.timeScale = 0f;
    }

    public void TriggerGameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        Time.timeScale = 0f;
        GOScoreText.text = score.ToString();
        GOHighScoreText.text = highScore.ToString();
        gameOverPanel.SetActive(true);

        PlayerPrefs.DeleteKey("SavedBallCount");
        PlayerPrefs.Save();


    }

    public void ReturnHome()
    {
        if (ballContainer != null)
        {
            foreach (Transform child in ballContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        gameUIPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        

    }


    private IEnumerator ResumeTimeWithDelay()
    {
        // Realtime ignores the fact that the game is "frozen"
        yield return new WaitForSecondsRealtime(0.8f);

        Time.timeScale = 1f; // Physics and Spawner logic "wake up" now
        UnityEngine.Debug.Log("Game Clock Started!");
    }

    public void NewGame()
    {
        

        if (ballContainer != null)
        {
            foreach (Transform child in ballContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        StartCoroutine(ResumeTimeWithDelay());
        isPaused = false;
        score = 0;
        UpdateScoreUI(score);
        mainMenuPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

    }

    public void ResumeGame()
    {
        StartCoroutine(ResumeTimeWithDelay());

        isPaused = false;

        score = PlayerPrefs.GetInt("SavedScore", 0);
        UpdateScoreUI(score);

        int savedCount = PlayerPrefs.GetInt("SavedBallCount", 0);
        for (int i = 0; i < savedCount; i++)
        {
            float x = PlayerPrefs.GetFloat($"Ball_{i}_X");
            float y = PlayerPrefs.GetFloat($"Ball_{i}_Y");
            int tierIndex = PlayerPrefs.GetInt($"Ball_{i}_TierIndex");

            // Use the index to grab the correct SO from your List
            if (tierIndex >= 0 && tierIndex < spawnableTiers.Count)
            {
                BallData data = spawnableTiers[tierIndex];

                GameObject newBall = Instantiate(ballTemplatePrefab, new Vector3(x, y, 0), Quaternion.identity);
                newBall.transform.SetParent(ballContainer.transform);

                // Re-initialize the ball with its physics and data
                newBall.GetComponent<BallInstance>().Setup(data);
                newBall.GetComponent<Rigidbody2D>().simulated = true;
            }
        }
        mainMenuPanel.SetActive(false);
        gameUIPanel.SetActive(true);
        
        
    }

    public void SaveGameState()
    {
        // 1. Clear old save data
        PlayerPrefs.SetInt("SavedBallCount", ballContainer.transform.childCount);
        PlayerPrefs.SetInt("SavedScore", score);

        // 2. Loop through your container (the one you set up earlier!)
        for (int i = 0; i < ballContainer.transform.childCount; i++)
        {
            Transform ball = ballContainer.transform.GetChild(i);
            BallData ballData = ball.GetComponent<BallInstance>().data; // Accessing your SO

            // Find the index of this SO in your evolution list
            int tierIndex = spawnableTiers.IndexOf(ballData);

            PlayerPrefs.SetFloat($"Ball_{i}_X", ball.position.x);
            PlayerPrefs.SetFloat($"Ball_{i}_Y", ball.position.y);
            PlayerPrefs.SetInt($"Ball_{i}_TierIndex", tierIndex);

           
        }
        PlayerPrefs.Save();
    }

    public GameObject settingspanel;
    public void ToggleSettings()
    {
        settingspanel.SetActive(true);
    }

    public void UntoggleSettings()
    {
        settingspanel.SetActive(false);
    }

    [Header("Audio Settings")]
    public AudioMixer mainMixer; // Drag your Audio Mixer here in the Inspector
    public Slider musicSlider;
    public Slider sfxSlider;
    public AudioSource sfxSource;

    public void SetMusicVolume(float sliderValue)
    {
        // Converts linear slider (0.0001 to 1) into logarithmic decibels (-80dB to 0dB)
        mainMixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);

        // Save the setting so it remembers for the next time they play
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    private void LoadAudioSettings()
    {
        // Get the saved volumes. If no save exists yet, default to 1 (max volume).
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Snap the UI sliders to the saved positions
        if (musicSlider != null) musicSlider.value = musicVol;
        if (sfxSlider != null) sfxSlider.value = sfxVol;

        // Apply the actual volume to the Audio Mixer
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }



}
