using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    private const float DefaultOrthographicSize = 5f;

    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float _followSpeed = 8f;

    private void Reset()
    {
        Camera cameraComponent = GetComponent<Camera>();

        if (cameraComponent != null)
        {
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = DefaultOrthographicSize;
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 desiredPosition = _target.position + _offset;
        float interpolationFactor = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, interpolationFactor);
    }
}
