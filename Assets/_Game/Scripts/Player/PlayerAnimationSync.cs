using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimationSync : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Animator _animator;

    private void Reset()
    {
        _movement = GetComponent<PlayerMovement>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (_movement == null)
        {
            _movement = GetComponent<PlayerMovement>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (_movement == null || _animator == null)
        {
            return;
        }

        Vector2 velocity = _movement.Velocity;

        _animator.SetBool("IsGrounded", _movement.IsGrounded);
        _animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        _animator.SetFloat("VerticalVelocity", velocity.y);
    }
}
