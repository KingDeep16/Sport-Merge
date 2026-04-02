using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BallSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject ballTemplatePrefab;
    public GameManager gameManager;
    private BallData _nextBallData;
    private BallData _pendingBallData; // The one waiting in limbo during the 0.8s cooldown

    [Header("Movement Bounds")]
    [SerializeField] private float leftWallX = -2f;
    [SerializeField] private float rightWallX = 3f;
    private bool _inputInitialized = false;
    public Image nextBallImageUI;

    void Start() => SpawnNextBall();

    private GameObject _currentBall;
    private bool _canDrop = true;
    private Vector2 _inputPosition;
    [Header("Movement Settings")]
    [SerializeField] private float lerpSpeed = 15f; // Higher = faster follow
    public GameObject BallContainer;
    private float _targetX;

    private float _inputDelayTimer = 0.2f; // Short delay
    private bool _isReady = false;

    [Header("Aiming Line")]
    public LineRenderer aimingLine;
    public LayerMask obstacleLayer;

    public void OnEnable()
    {
        _isReady = false;
        _inputDelayTimer = 0.2f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        // 1. Safety Check (Pause/Game State)
        if (Time.timeScale == 0) return;

        if (!_isReady)
        {
            _inputDelayTimer -= Time.deltaTime;
            if (_inputDelayTimer <= 0) _isReady = true;
            return; // Skip input handling until ready
        }

        // 2. --- AIMING LINE LOGIC ---
        if (_currentBall != null && _canDrop)
        {
            // Turn the line on
            aimingLine.enabled = true;
            Vector2 ballPos = _currentBall.transform.position;

            // Point A: The center of the ball you are holding
            aimingLine.SetPosition(0, ballPos);

            // Shoot an invisible laser straight down
            RaycastHit2D hit = Physics2D.Raycast(ballPos, Vector2.down, 20f, obstacleLayer);

            if (hit.collider != null)
            {
                // Point B: Exactly where the laser hits a dropped ball or the floor
                aimingLine.SetPosition(1, hit.point);
            }
            else
            {
                // Fallback Point B: If it hits nothing, just draw it far down
                aimingLine.SetPosition(1, ballPos + (Vector2.down * 15f));
            }
        }
        else
        {
            // Turn the line off if we aren't holding a ball
            if (aimingLine != null) aimingLine.enabled = false;
        }

        // 2. Get the New Input Data
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        // 3. Handle Movement (X Position)
        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            _inputPosition = touch.primaryTouch.position.ReadValue();
            UpdateTargetX(_inputPosition);
        }
        else if (mouse != null && mouse.leftButton.isPressed)
        {
            _inputPosition = mouse.position.ReadValue();
            UpdateTargetX(_inputPosition);
        }

        // 4. Handle Dropping (The "Up" event)
        bool touchReleased = touch != null && touch.primaryTouch.press.wasReleasedThisFrame;
        bool mouseReleased = mouse != null && mouse.leftButton.wasReleasedThisFrame;

        if ((touchReleased || mouseReleased) && _canDrop)
        {
            StartCoroutine(DropSequence());
        }

        // 5. Apply the Smooth Movement (Keep your existing Lerp logic)
        MoveSpawner();

    // 2. Calculate Radius for Clamping
    float currentRadius = 0.2f;
        if (_currentBall != null)
        {
            currentRadius = _currentBall.GetComponent<CircleCollider2D>().radius * _currentBall.transform.localScale.x;
        }

        // 3. Clamp the Target (not the actual position yet)
        float clampedTargetX = Mathf.Clamp(_targetX, leftWallX + currentRadius, rightWallX - currentRadius);

        // 4. THE ANIMATION: Smoothly move the spawner toward the clamped target
        float smoothedX = Mathf.Lerp(transform.position.x, clampedTargetX, Time.deltaTime * lerpSpeed);

        transform.position = new Vector3(smoothedX, transform.position.y, 0);
    }

    void UpdateTargetX(Vector2 screenPos)
    {
        _targetX = Camera.main.ScreenToWorldPoint(screenPos).x;
    }

    void MoveSpawner()
    {
        float currentRadius = (_currentBall != null) ?
            _currentBall.GetComponent<CircleCollider2D>().radius * _currentBall.transform.localScale.x : 0.2f;

        float clampedX = Mathf.Clamp(_targetX, leftWallX + currentRadius, rightWallX - currentRadius);
        transform.position = new Vector3(Mathf.Lerp(transform.position.x, clampedX, Time.deltaTime * lerpSpeed), transform.position.y, 0);
    }

   
public void SpawnNextBall()
    {
        if (gameManager == null || gameManager.spawnableTiers.Count == 0)
        {
            Debug.LogWarning("Waiting for GameManager to initialize...");
            return;
        }
        if (_nextBallData == null)
        {
            // Prime the pump for the very first drop
            int firstIndex = Random.Range(0, 3);
            _pendingBallData = gameManager.spawnableTiers[firstIndex];

            int nextIndex = Random.Range(0, 3);
            _nextBallData = gameManager.spawnableTiers[nextIndex];
            nextBallImageUI.sprite = _nextBallData.ballSprite;
        }


        _currentBall = Instantiate(ballTemplatePrefab, transform.position, Quaternion.identity);
        _currentBall.transform.SetParent(this.transform);

        Rigidbody2D rb = _currentBall.GetComponent<Rigidbody2D>();
        rb.simulated = false; // Disable physics while holding

        _currentBall.GetComponent<BallInstance>().Setup(_pendingBallData);
    }

    IEnumerator DropSequence()
    {
        _canDrop = false;
        _currentBall.transform.SetParent(BallContainer.transform);
        _currentBall.GetComponent<Rigidbody2D>().simulated = true;
        _currentBall = null;

        yield return new WaitForSeconds(0.8f);
        _pendingBallData = _nextBallData;

        // 2. Roll a brand new ball for the UI so the player can see it during the cooldown
        int randomIndex = Random.Range(0, 3);
        _nextBallData = gameManager.spawnableTiers[randomIndex];
        nextBallImageUI.sprite = _nextBallData.ballSprite;

        yield return new WaitForSeconds(0.2f); // Cooldown for polish
        SpawnNextBall();
        _canDrop = true;
        _inputInitialized = false;
    }
}



