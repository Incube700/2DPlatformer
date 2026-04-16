using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [SerializeField] private Collider2D _triggerCollider;

    private void Reset()
    {
        _triggerCollider = GetComponent<Collider2D>();

        if (_triggerCollider != null)
        {
            _triggerCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        if (_triggerCollider == null)
        {
            _triggerCollider = GetComponent<Collider2D>();
        }

        if (_triggerCollider != null && _triggerCollider.isTrigger == false)
        {
            _triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerDeathHandler playerDeathHandler = other.GetComponentInParent<PlayerDeathHandler>();

        if (playerDeathHandler == null)
        {
            return;
        }

        playerDeathHandler.Kill();
    }
}
