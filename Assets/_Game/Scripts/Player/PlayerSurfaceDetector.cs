using UnityEngine;

public class PlayerSurfaceDetector : MonoBehaviour
{
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Collider2D _playerCollider;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _groundCheckRadius = 0.18f;

    [SerializeField] private float _wallCheckDistance = 0.08f;
    [SerializeField] private float _wallCheckHeightPadding = 0.1f;

    private int _facingDirection = 1;
    private bool _hasShownGroundCheckWarning;
    private bool _hasShownGroundLayerMaskWarning;
    private bool _hasShownPlayerColliderWarning;

    private void Reset()
    {
        _groundCheck = transform.Find("GroundCheck");
        _playerCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (_groundCheck == null)
        {
            _groundCheck = transform.Find("GroundCheck");
        }

        if (_playerCollider == null)
        {
            _playerCollider = GetComponent<Collider2D>();
        }
    }

    public bool IsGrounded()
    {
        if (CanCheckGround() == false)
        {
            return false;
        }

        return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayerMask) != null;
    }

    public bool IsTouchingWall()
    {
        if (CanCheckWall() == false)
        {
            return false;
        }

        Vector2 checkSize = GetWallCheckSize();
        Vector2 checkCenter = GetWallCheckCenter(checkSize);

        return Physics2D.OverlapBox(checkCenter, checkSize, 0f, _groundLayerMask) != null;
    }

    public bool TryGetWallDirection(out int wallDirection)
    {
        if (IsTouchingWall() == false)
        {
            wallDirection = 0;
            return false;
        }

        wallDirection = _facingDirection;
        return true;
    }

    public void SetFacingDirection(int facingDirection)
    {
        if (facingDirection == 0)
        {
            return;
        }

        _facingDirection = facingDirection > 0 ? 1 : -1;
    }

    private bool CanCheckGround()
    {
        if (_groundCheck == null)
        {
            ShowGroundCheckWarning();
            return false;
        }

        if (HasGroundLayerMask() == false)
        {
            return false;
        }

        return true;
    }

    private bool CanCheckWall()
    {
        if (_playerCollider == null)
        {
            ShowPlayerColliderWarning();
            return false;
        }

        if (HasGroundLayerMask() == false)
        {
            return false;
        }

        return true;
    }

    private bool HasGroundLayerMask()
    {
        if (_groundLayerMask.value != 0)
        {
            return true;
        }

        if (_hasShownGroundLayerMaskWarning == false)
        {
            _hasShownGroundLayerMaskWarning = true;
            Debug.LogWarning($"{nameof(PlayerSurfaceDetector)} on {name} requires Ground Layer Mask to be assigned in the inspector.", this);
        }

        return false;
    }

    private Vector2 GetWallCheckSize()
    {
        Bounds bounds = _playerCollider.bounds;

        float height = Mathf.Max(0.1f, bounds.size.y - _wallCheckHeightPadding);

        return new Vector2(_wallCheckDistance, height);
    }

    private Vector2 GetWallCheckCenter(Vector2 checkSize)
    {
        Bounds bounds = _playerCollider.bounds;

        float centerX = bounds.center.x + (bounds.extents.x + checkSize.x * 0.5f) * _facingDirection;
        float centerY = bounds.center.y;

        return new Vector2(centerX, centerY);
    }

    private void ShowGroundCheckWarning()
    {
        if (_hasShownGroundCheckWarning)
        {
            return;
        }

        _hasShownGroundCheckWarning = true;
        Debug.LogWarning($"{nameof(PlayerSurfaceDetector)} on {name} requires Ground Check transform.", this);
    }

    private void ShowPlayerColliderWarning()
    {
        if (_hasShownPlayerColliderWarning)
        {
            return;
        }

        _hasShownPlayerColliderWarning = true;
        Debug.LogWarning($"{nameof(PlayerSurfaceDetector)} on {name} requires player collider.", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }

        if (_playerCollider != null)
        {
            Vector2 checkSize = GetWallCheckSize();
            Vector2 checkCenter = GetWallCheckCenter(checkSize);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(checkCenter, checkSize);
        }
    }
}