using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxMultiplier = 0.5f;

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition;

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
        transform.position += deltaMovement * parallaxMultiplier;
        _lastCameraPosition = _cameraTransform.position;
    }
}