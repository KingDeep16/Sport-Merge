using UnityEngine;

[CreateAssetMenu(fileName = "NewBallData", menuName = "Ball Data")]
public class BallData : ScriptableObject
{
    public string ballName;
    public Sprite ballSprite;
    public float scale = 1f;
    public float mass = 1f;
    public int scoreValue = 10;
    public int NextIndex;
    public BallData nextTier; // Drag the next ball in the sequence here
}