using UnityEngine;

[ExecuteInEditMode] // This lets us see the changes in the Scene view without pressing Play!
public class CameraScaler : MonoBehaviour
{
    [SerializeField] private float sceneWidth = 10f; // Adjust this to the width of your 'Bin'
    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Calculate how much units per pixel we need
        // 1.0f / _cam.aspect gives us the ratio of height to width
        float unitsPerPixel = sceneWidth / _cam.pixelWidth;

        float desiredHalfHeight = 0.5f * unitsPerPixel * _cam.pixelHeight;

        _cam.orthographicSize = desiredHalfHeight;
    }
}