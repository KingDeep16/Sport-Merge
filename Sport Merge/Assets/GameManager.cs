using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton pattern for easy access
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject gameUIPanel;
    private bool _isGameStarted = false;
    private bool _isGameOver = false;

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
        Debug.Log("GAME OVER!");

        // Potential Next Step: Show a UI panel here
        // For now, let's just restart after a delay
        Invoke("RestartGame", 3f);
    }

    public void ReturnHome()
    {
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        gameUIPanel.SetActive(false);
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        // Start the delay sequence so the "Play" click doesn't drop a ball
        StartCoroutine(ResumeTimeWithDelay());
    }

    private IEnumerator ResumeTimeWithDelay()
    {
        // Realtime ignores the fact that the game is "frozen"
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f; // Physics and Spawner logic "wake up" now
        Debug.Log("Game Clock Started!");
    }
}
