using UnityEngine;

public class GameOverLine : MonoBehaviour
{
    private float _timer = 0f;
    [SerializeField] private float timeToFail = 2.0f; // Must stay above line for 2 seconds

    void OnTriggerStay2D(Collider2D other)
    {
        // Only trigger for balls that are NOT currently attached to the spawner
        if (other.CompareTag("Ball") && other.attachedRigidbody.simulated)
        {
            _timer += Time.deltaTime;

            if (_timer >= timeToFail)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Reset the timer if the ball falls back down
        _timer = 0f;
    }
}