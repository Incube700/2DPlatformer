using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _platformCollider;
    [SerializeField] private float _fallDelay = 0.75f;
    [SerializeField] private float _fallGravityScale = 2f;

    private bool _isTriggered;

    private void Reset()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _platformCollider = GetComponent<Collider2D>();
        PrepareIdleState();
    }

    private void Awake()
    {
        if (_rigidbody2D == null)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (_platformCollider == null)
        {
            _platformCollider = GetComponent<Collider2D>();
        }

        PrepareIdleState();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryActivate(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryActivate(collision.collider);
    }

    private void TryActivate(Collider2D other)
    {
        if (_isTriggered)
        {
            return;
        }

        PlayerDeathHandler playerDeathHandler = other.GetComponentInParent<PlayerDeathHandler>();

        if (playerDeathHandler == null || playerDeathHandler.IsDead)
        {
            return;
        }

        if (IsPlayerStandingOnTop(other) == false)
        {
            return;
        }

        _isTriggered = true;
        StartCoroutine(FallCoroutine());
    }

    private IEnumerator FallCoroutine()
    {
        if (_fallDelay > 0f)
        {
            yield return new WaitForSeconds(_fallDelay);
        }

        if (_rigidbody2D == null)
        {
            yield break;
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.gravityScale = _fallGravityScale;
    }

    private bool IsPlayerStandingOnTop(Collider2D playerCollider)
    {
        if (_platformCollider == null)
        {
            return false;
        }

        float playerFeetHeight = playerCollider.bounds.min.y;
        float platformCenterHeight = _platformCollider.bounds.center.y;

        return playerFeetHeight >= platformCenterHeight;
    }

    private void PrepareIdleState()
    {
        if (_rigidbody2D == null)
        {
            return;
        }

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.freezeRotation = true;
        _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
}
