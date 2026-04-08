using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ReviveUIController : MonoBehaviour
{
    public static ReviveUIController Instance; // Singleton for easy access

    [Header("UI References")]
    public GameObject revivePanel;
    public GameObject gameOverPanel; // The actual "You Lose" screen
    public TextMeshProUGUI timerText;
    public Button watchAdButton;
    public Button skipButton;

    [Header("Timer Settings")]
    public float countdownTime = 5f; // 5 seconds is standard!
    private float currentTime;
    private bool isCounting = false;

    private void Awake()
    {
        // If the container is empty, put THIS script inside it
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // If another instance already exists, destroy this one so we don't have duplicates
            Destroy(gameObject);
        }
    }

    // Call this method when the player "dies"
    public void ShowRevivePopup()
    {
        Time.timeScale = 0f; // Pause the game
        revivePanel.SetActive(true);
        gameOverPanel.SetActive(false);

        currentTime = countdownTime;
        isCounting = true;
        skipButton.interactable = true;

        if (AdsManager.Instance != null && AdsManager.Instance.IsRewardedAdReady())
        {
            watchAdButton.interactable = true;
        }
        else
        {
            watchAdButton.interactable = false;
        }
    }

    private void Update()
    {
        if (isCounting)
        {
            currentTime -= Time.unscaledDeltaTime;

            // Mathf.CeilToInt ensures 4.9 seconds displays as "5" instead of "4"
            timerText.text = Mathf.CeilToInt(currentTime).ToString();

            if (currentTime <= 0)
            {
                TriggerGameOver(); // Time ran out!
            }
        }
    }

    // Hook this to your "Watch Ad" Button in the Inspector
    public void OnWatchAdClicked()
    {
        isCounting = false; // Pause the timer
        watchAdButton.interactable = false; // Prevent double-clicking
        skipButton.interactable = false;

        // Tell the AdsManager to play the video!
        AdsManager.Instance.ShowRewardedAd();
    }

    // Hook this to your "Skip" Button in the Inspector
    public void TriggerGameOver()
    {
        isCounting = false;
        revivePanel.SetActive(false); // Hide this popup
       GameManager.Instance.TriggerGameOver();
    }
}

