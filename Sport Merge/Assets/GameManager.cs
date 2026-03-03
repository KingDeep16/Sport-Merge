using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton pattern for easy access

    private bool _isGameOver = false;

    void Awake()
    {
        // Simple Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}