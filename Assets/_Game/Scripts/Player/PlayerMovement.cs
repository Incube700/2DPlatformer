using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerSurfaceDetector))]
public class PlayerMovement : MonoBehaviour
{
    private const float InputThreshold = 0.01f;

    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private PlayerSurfaceDetector _surfaceDetector;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private SpriteRenderer _visualSpriteRenderer;

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _groundAcceleration = 90f;
    [SerializeField] private float _groundDeceleration = 110f;
    [SerializeField] private float _airAcceleration = 60f;
    [SerializeField] private float _airDeceleration = 65f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 14f;
    [SerializeField] private Vector2 _wallJumpForce = new Vector2(10f, 14f);
    [SerializeField] private float _wallJumpInputLockTime = 0.15f;
    [SerializeField] private float _coyoteTime = 0.12f;
    [SerializeField] private float _jumpBufferTime = 0.12f;
    [SerializeField] private float _fallGravityMultiplier = 2.4f;
    [SerializeField] private float _lowJumpGravityMultiplier = 3.2f;
    [SerializeField] private float _wallSlideSpeed = 2.5f;

    private float _horizontalInput;
    private bool _isJumpHeld;
    private bool _isGrounded;
    private bool _isTouchingWall;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _wallJumpLockTimer;
    private float _baseGravityScale;
    private int _facingDirection;
    private Vector3 _visualStartScale;
    private bool _hasShownVisualSpriteWarning;

    public bool IsGrounded => _isGrounded;

    public Vector2 Velocity => _rigidbody2D != null ? _rigidbody2D.velocity : Vector2.zero;

    private void Reset()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _surfaceDetector = GetComponent<PlayerSurfaceDetector>();
        _visualRoot = transform;
        _visualSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_rigidbody2D != null)
        {
            _rigidbody2D.freezeRotation = true;
            _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Awake()
    {
        ResolveDependencies();

        _baseGravityScale = _rigidbody2D != null ? _rigidbody2D.gravityScale : 1f;
        _visualStartScale = _visualRoot != null ? _visualRoot.localScale : Vector3.one;

        SetFacingDirection(1);
    }

    private void Update()
    {
        ReadInput();
        RefreshSurfaceState();
        UpdateTimers();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        RefreshSurfaceState();
        TryConsumeJumpBuffer();
        ApplyHorizontalMovement();
        ApplyWallSlide();
        ApplyGravityScale();
    }

    private void ReadInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        _isJumpHeld = Input.GetButton("Jump");
    }

    private void RefreshSurfaceState()
    {
        if (_surfaceDetector == null)
        {
            _isGrounded = false;
            _isTouchingWall = false;
            return;
        }

        _isGrounded = _surfaceDetector.IsGrounded();
        _isTouchingWall = _surfaceDetector.IsTouchingWall();
    }

    private void UpdateTimers()
    {
        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        if (_jumpBufferTimer > 0f)
        {
            _jumpBufferTimer -= Time.deltaTime;
        }

        if (_wallJumpLockTimer > 0f)
        {
            _wallJumpLockTimer -= Time.deltaTime;
        }
    }

    private void UpdateFacingDirection()
    {
        if (Mathf.Abs(_horizontalInput) < InputThreshold)
        {
            return;
        }

        int targetDirection = _horizontalInput > 0f ? 1 : -1;
        SetFacingDirection(targetDirection);
    }

    private void SetFacingDirection(int direction)
    {
        if (direction == 0 || direction == _facingDirection)
        {
            return;
        }

        _facingDirection = direction > 0 ? 1 : -1;

        if (_surfaceDetector != null)
        {
            _surfaceDetector.SetFacingDirection(_facingDirection);
        }

        UpdateVisualFacing();
    }

    private void TryConsumeJumpBuffer()
    {
        if (_jumpBufferTimer <= 0f || _rigidbody2D == null)
        {
            return;
        }

        if (CanGroundJump())
        {
            PerformGroundJump();
            return;
        }

        if (CanWallJump())
        {
            PerformWallJump();
        }
    }

    private bool CanGroundJump()
    {
        return _coyoteTimer > 0f;
    }

    private bool CanWallJump()
    {
        return _isGrounded == false && _isTouchingWall;
    }

    private void PerformGroundJump()
    {
        Vector2 velocity = _rigidbody2D.velocity;
        velocity.y = _jumpForce;
        _rigidbody2D.velocity = velocity;

        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
    }

    private void PerformWallJump()
    {
        if (_surfaceDetector == null)
        {
            return;
        }

        if (_surfaceDetector.TryGetWallDirection(out int wallDirection) == false)
        {
            return;
        }

        Vector2 velocity = _rigidbody2D.velocity;
        velocity.x = -wallDirection * _wallJumpForce.x;
        velocity.y = _wallJumpForce.y;
        _rigidbody2D.velocity = velocity;

        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
        _wallJumpLockTimer = _wallJumpInputLockTime;

        SetFacingDirection(-wallDirection);
    }

    private void ApplyHorizontalMovement()
    {
        if (_rigidbody2D == null || _wallJumpLockTimer > 0f)
        {
            return;
        }

        float targetSpeed = _horizontalInput * _moveSpeed;
        float acceleration = GetHorizontalAcceleration(targetSpeed);
        float newVelocityX = Mathf.MoveTowards(_rigidbody2D.velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);

        _rigidbody2D.velocity = new Vector2(newVelocityX, _rigidbody2D.velocity.y);
    }

    private float GetHorizontalAcceleration(float targetSpeed)
    {
        if (Mathf.Abs(targetSpeed) > InputThreshold)
        {
            return _isGrounded ? _groundAcceleration : _airAcceleration;
        }

        return _isGrounded ? _groundDeceleration : _airDeceleration;
    }

    private void ApplyWallSlide()
    {
        if (_rigidbody2D == null)
        {
            return;
        }

        if (_isGrounded || _isTouchingWall == false)
        {
            return;
        }

        if (_rigidbody2D.velocity.y >= 0f)
        {
            return;
        }

        float clampedVelocityY = Mathf.Max(_rigidbody2D.velocity.y, -_wallSlideSpeed);
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, clampedVelocityY);
    }

    private void ApplyGravityScale()
    {
        if (_rigidbody2D == null)
        {
            return;
        }

        float gravityMultiplier = 1f;

        if (_isGrounded == false)
        {
            if (_rigidbody2D.velocity.y < 0f)
            {
                gravityMultiplier = _fallGravityMultiplier;
            }
            else if (_rigidbody2D.velocity.y > 0f && _isJumpHeld == false)
            {
                gravityMultiplier = _lowJumpGravityMultiplier;
            }
        }

        _rigidbody2D.gravityScale = _baseGravityScale * gravityMultiplier;
    }

    private void ResolveDependencies()
    {
        if (_rigidbody2D == null)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (_surfaceDetector == null)
        {
            _surfaceDetector = GetComponent<PlayerSurfaceDetector>();
        }

        if (_visualRoot == null)
        {
            _visualRoot = transform;
        }

        if (_visualSpriteRenderer == null)
        {
            _visualSpriteRenderer = _visualRoot.GetComponent<SpriteRenderer>();
        }

        if (_visualSpriteRenderer == null)
        {
            _visualSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void UpdateVisualFacing()
    {
        if (_visualRoot == null)
        {
            return;
        }

        if (_visualRoot == transform)
        {
            UpdateSpriteFacing();
            return;
        }

        Vector3 scale = _visualStartScale;
        scale.x = Mathf.Abs(_visualStartScale.x) * _facingDirection;
        _visualRoot.localScale = scale;
    }

    private void UpdateSpriteFacing()
    {
        if (_visualSpriteRenderer == null)
        {
            ShowVisualSpriteWarning();
            return;
        }

        _visualSpriteRenderer.flipX = _facingDirection < 0;
    }

    private void ShowVisualSpriteWarning()
    {
        if (_hasShownVisualSpriteWarning)
        {
            return;
        }

        _hasShownVisualSpriteWarning = true;
        Debug.LogWarning($"{nameof(PlayerMovement)} on {name} could not find SpriteRenderer for visual flip. Assign Visual Root or Visual Sprite Renderer in the inspector.", this);
    }
}
