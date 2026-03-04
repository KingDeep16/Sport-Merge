using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject ballTemplatePrefab;
    [SerializeField] private List<BallData> spawnableTiers; // Tiers 0, 1, and 2

    [Header("Movement Bounds")]
    [SerializeField] private float leftWallX = -2f;
    [SerializeField] private float rightWallX = 3f;
    private bool _inputInitialized = false;

    private GameObject _currentBall;
    private bool _canDrop = true;

    void Start() => SpawnNextBall();

    [Header("Movement Settings")]
    [SerializeField] private float lerpSpeed = 15f; // Higher = faster follow

    private float _targetX;

    void Update()
    {
        // 1. If we aren't in play mode, stay reset
        if (Time.timeScale == 0)
        {
            _inputInitialized = false;
            return;
        }

        // 2. The "Safety Gate": Don't allow a drop until the user has 
        // actually started a NEW touch/click AFTER the menu is gone.
        if (Input.GetMouseButtonDown(0))
        {
            _inputInitialized = true;
        }

        // 3. Only proceed with movement/dropping if the input started after play began
        if (!_inputInitialized) return;

        // 1. Capture the Target X position based on Input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            _targetX = Camera.main.ScreenToWorldPoint(touch.position).x;

            if (touch.phase == TouchPhase.Ended && _canDrop)
                StartCoroutine(DropSequence());
        }
        else if (Input.GetMouseButton(0))
        {
            _targetX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        }

        if (Input.GetMouseButtonUp(0) && _canDrop)
            StartCoroutine(DropSequence());

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

    void SpawnNextBall()
    {
        int randomIndex = Random.Range(0, 3);
        BallData data = spawnableTiers[randomIndex];

        _currentBall = Instantiate(ballTemplatePrefab, transform.position, Quaternion.identity);
        _currentBall.transform.SetParent(this.transform);

        Rigidbody2D rb = _currentBall.GetComponent<Rigidbody2D>();
        rb.simulated = false; // Disable physics while holding

        _currentBall.GetComponent<BallInstance>().Setup(data);
    }

    IEnumerator DropSequence()
    {
        _canDrop = false;
        _currentBall.transform.SetParent(null);
        _currentBall.GetComponent<Rigidbody2D>().simulated = true;
        _currentBall = null;

        yield return new WaitForSeconds(0.8f); // Cooldown for polish
        SpawnNextBall();
        _canDrop = true;
        _inputInitialized = false;
    }
}