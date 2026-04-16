using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardKillOnTouch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKillPlayer(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKillPlayer(collision.collider);
    }

    private void TryKillPlayer(Component other)
    {
        PlayerDeathHandler playerDeathHandler = other.GetComponentInParent<PlayerDeathHandler>();

        if (playerDeathHandler == null)
        {
            return;
        }

        playerDeathHandler.Kill();
    }
}
