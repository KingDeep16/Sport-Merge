using Unity.VisualScripting;
using UnityEngine;

public class BallInstance : MonoBehaviour
{
    public BallData data;
    private CircleCollider2D _collider;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(BallData newData)
    {
        data = newData;
        _spriteRenderer.sprite = data.ballSprite;

        // 1. Reset scale to 1,1,1 first so our measurements are 'clean'
        transform.localScale = Vector3.one;

        // 2. Calculate the 'Local' radius based on the Sprite's actual bounds
        // Sprite.bounds.extents.x is half the width of the sprite in Unity Units
        float spriteRadius = _spriteRenderer.sprite.bounds.extents.x;
        _collider.radius = spriteRadius;

        // 3. Apply the 'Game Scale' from your ScriptableObject
        // This allows you to make the Basketball physically bigger than the Tennis ball
        transform.localScale = Vector3.one * data.scale;

        // 4. Update Physics properties
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.mass = data.mass;
    }


    private bool _isMerged = false; // Prevents multiple merges in one frame

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Check if we hit another ball
        if (collision.gameObject.TryGetComponent<BallInstance>(out BallInstance otherBall))
        {
            // 2. Do they have the same 'BallData'? (Must be the same sports ball)
            if (this.data == otherBall.data)
            {
                // 3. Safety checks
                if (_isMerged || otherBall._isMerged) return;
                if (data.nextTier == null) return; // Already at the largest ball!

                // 4. The "Winner" check
                // Only the ball with the higher InstanceID handles the spawning
                if (this.gameObject.GetInstanceID() < otherBall.gameObject.GetInstanceID())
                {
                    PerformMerge(otherBall);
                }
            }
        }
    }

    private void PerformMerge(BallInstance other)
    {
        _isMerged = true;
        other._isMerged = true;

        // Calculate the midpoint between the two balls
        Vector3 spawnPos = (transform.position + other.transform.position) / 2f;

        // Spawn the new ball (using your existing Template Prefab)
        GameObject newBallObj = Instantiate(gameObject, spawnPos, Quaternion.identity);
        BallInstance newBallScript = newBallObj.GetComponent<BallInstance>();

        // Initialize it with the NEXT tier data
        newBallScript.Setup(data.nextTier);

        newBallObj.transform.SetParent(this.gameObject.transform.parent);

        // Ensure the new ball has physics enabled immediately
        newBallObj.GetComponent<Rigidbody2D>().simulated = true;

        // Destroy the two old balls
        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }
}