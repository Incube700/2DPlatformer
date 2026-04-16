using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerAnimationSync _animationSync;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Collider2D _collider2D;
    [SerializeField] private LevelRestarter _levelRestarter;

    private bool _isDead;

    public bool IsDead => _isDead;

    private void Reset()
    {
        _movement = GetComponent<PlayerMovement>();
        _animationSync = GetComponent<PlayerAnimationSync>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _levelRestarter = FindFirstObjectByType<LevelRestarter>();
    }

    private void Awake()
    {
        if (_movement == null)
        {
            _movement = GetComponent<PlayerMovement>();
        }

        if (_animationSync == null)
        {
            _animationSync = GetComponent<PlayerAnimationSync>();
        }

        if (_rigidbody2D == null)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (_collider2D == null)
        {
            _collider2D = GetComponent<Collider2D>();
        }

        if (_levelRestarter == null)
        {
            _levelRestarter = FindFirstObjectByType<LevelRestarter>();
        }
    }

    public void Kill()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        if (_movement != null)
        {
            _movement.enabled = false;
        }

        if (_animationSync != null)
        {
            _animationSync.enabled = false;
        }

        if (_rigidbody2D != null)
        {
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.simulated = false;
        }

        if (_collider2D != null)
        {
            _collider2D.enabled = false;
        }

        if (_levelRestarter != null)
        {
            _levelRestarter.RestartLevel();
            return;
        }

        Debug.LogWarning("LevelRestarter is not assigned on PlayerDeathHandler.");
    }
}
