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
        if (TryGetPlayerDeathHandler(other, out PlayerDeathHandler playerDeathHandler) == false)
        {
            return;
        }

        playerDeathHandler.Kill();
    }

    private bool TryGetPlayerDeathHandler(Component other, out PlayerDeathHandler playerDeathHandler)
    {
        playerDeathHandler = other.GetComponentInParent<PlayerDeathHandler>();
        return playerDeathHandler != null;
    }
}
