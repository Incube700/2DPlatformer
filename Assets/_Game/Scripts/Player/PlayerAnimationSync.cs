using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimationSync : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Animator _animator;

    private bool _hasShownAnimatorWarning;

    private void Reset()
    {
        _movement = GetComponent<PlayerMovement>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        ResolveDependencies();
    }

    private void Update()
    {
        if (_movement == null)
        {
            return;
        }

        if (HasAnimator() == false)
        {
            return;
        }

        Vector2 velocity = _movement.Velocity;

        _animator.SetBool("IsGrounded", _movement.IsGrounded);
        _animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        _animator.SetFloat("VerticalVelocity", velocity.y);
    }

    private void ResolveDependencies()
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

    private bool HasAnimator()
    {
        if (_animator != null)
        {
            return true;
        }

        if (_hasShownAnimatorWarning == false)
        {
            _hasShownAnimatorWarning = true;
            Debug.LogWarning($"{nameof(PlayerAnimationSync)} on {name} requires Animator to be assigned or present in children.", this);
        }

        return false;
    }
}
