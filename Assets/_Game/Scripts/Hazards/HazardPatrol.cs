using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HazardPatrol : MonoBehaviour
{
    private const float MinCycleDuration = 0.01f;
    private const float GizmoPointRadius = 0.15f;

    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private float _cycleDuration = 2f;
    [SerializeField] private AnimationCurve _positionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float _rotationSpeed = -360f;

    private float _elapsedTime;

    private void Reset()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _visualRoot = transform;
        ConfigureRigidBody();
    }

    private void Awake()
    {
        if (_rigidbody2D == null)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (_visualRoot == null)
        {
            _visualRoot = transform;
        }

        ConfigureRigidBody();
    }

    private void Update()
    {
        if (_visualRoot == null || Mathf.Approximately(_rotationSpeed, 0f))
        {
            return;
        }

        _visualRoot.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (_rigidbody2D == null || _pointA == null || _pointB == null)
        {
            return;
        }

        float duration = Mathf.Max(_cycleDuration, MinCycleDuration);
        _elapsedTime += Time.fixedDeltaTime;

        float normalizedTime = Mathf.PingPong(_elapsedTime / duration, 1f);
        float curvedTime = _positionCurve.Evaluate(normalizedTime);
        Vector2 nextPosition = Vector2.Lerp(_pointA.position, _pointB.position, curvedTime);

        _rigidbody2D.MovePosition(nextPosition);
    }

    private void ConfigureRigidBody()
    {
        if (_rigidbody2D == null)
        {
            return;
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnDrawGizmosSelected()
    {
        if (_pointA == null || _pointB == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(_pointA.position, _pointB.position);
        Gizmos.DrawWireSphere(_pointA.position, GizmoPointRadius);
        Gizmos.DrawWireSphere(_pointB.position, GizmoPointRadius);
    }
}
