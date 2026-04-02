using UnityEngine;

[CreateAssetMenu(fileName = "NewBallData", menuName = "Ball Data")]
public class BallData : ScriptableObject
{
    [Header("Polish")]
    public AudioClip mergeSound;      // The sound to play when this ball is created
    public GameObject mergeParticles;

    public string ballName;
    public Sprite ballSprite;
    public float scale = 1f;
    public float mass = 1f;
    public int scoreValue = 10;
    public int NextIndex;
    public BallData nextTier; // Drag the next ball in the sequence here
}