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
}