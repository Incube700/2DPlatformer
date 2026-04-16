using UnityEngine;

public class PlayerSurfaceDetector : MonoBehaviour
{
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private LayerMask _groundLayerMask = ~0;
    [SerializeField] private float _groundCheckRadius = 0.18f;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.2f, 0.9f);

    private Vector3 _wallCheckStartLocalPosition;
    private int _facingDirection = 1;

    private void Reset()
    {
        _groundCheck = transform.Find("GroundCheck");
        _wallCheck = transform.Find("WallCheck");
        CacheWallCheckLocalPosition();
    }

    private void Awake()
    {
        if (_groundCheck == null)
        {
            _groundCheck = transform.Find("GroundCheck");
        }

        if (_wallCheck == null)
        {
            _wallCheck = transform.Find("WallCheck");
        }

        CacheWallCheckLocalPosition();
        SetFacingDirection(_facingDirection);
    }

    public bool IsGrounded()
    {
        if (_groundCheck == null)
        {
            return false;
        }

        return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayerMask) != null;
    }

    public bool IsTouchingWall()
    {
        if (_wallCheck == null)
        {
            return false;
        }

        return Physics2D.OverlapBox(_wallCheck.position, _wallCheckSize, 0f, _groundLayerMask) != null;
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

        if (_wallCheck == null)
        {
            return;
        }

        Vector3 localPosition = _wallCheckStartLocalPosition;
        localPosition.x = Mathf.Abs(_wallCheckStartLocalPosition.x) * _facingDirection;
        _wallCheck.localPosition = localPosition;
    }

    private void CacheWallCheckLocalPosition()
    {
        if (_wallCheck == null)
        {
            return;
        }

        _wallCheckStartLocalPosition = _wallCheck.localPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }

        if (_wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_wallCheck.position, _wallCheckSize);
        }
    }
}
