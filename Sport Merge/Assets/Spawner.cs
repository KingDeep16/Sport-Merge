using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject ballTemplatePrefab; // The "Template" ball
    [SerializeField] private List<BallData> ballTierList;   // List of all sports balls

    private GameObject _currentBall;
    private bool _canDrop = true;

    void Start()
    {
        SpawnNextBall();
    }

    void Update()
    {
        // Follow Mouse logic (clamped)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float clampedX = Mathf.Clamp(mousePos.x, -2.2f, 2.2f);
        transform.position = new Vector3(clampedX, transform.position.y, 0);

        if (Input.GetMouseButtonDown(0) && _canDrop)
        {
            StartCoroutine(DropSequence());
        }
    }

    void SpawnNextBall()
    {
        int randomIndex = Random.Range(0, 3);
        BallData data = ballTierList[randomIndex];

        _currentBall = Instantiate(ballTemplatePrefab, transform.position, Quaternion.identity);
        _currentBall.transform.SetParent(this.transform);

        // THE FIX:
        Rigidbody2D rb = _currentBall.GetComponent<Rigidbody2D>();
        rb.simulated = false; // Turn off physics while we are holding it

        _currentBall.GetComponent<BallInstance>().Setup(data);
    }

    System.Collections.IEnumerator DropSequence()
    {
        _canDrop = false;

        _currentBall.transform.SetParent(null); // Detach from spawner
        _currentBall.GetComponent<Rigidbody2D>().simulated = true;
        _currentBall = null;

        yield return new WaitForSeconds(1.0f); // Delay before next ball
        SpawnNextBall();
        _canDrop = true;
    }
}
