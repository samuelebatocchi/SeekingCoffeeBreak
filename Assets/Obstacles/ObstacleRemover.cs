using UnityEngine;

public class ObstacleRemover : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Non serve più differenziare i tag per la rimozione, 
        // MasterSpawner sa a quale pool appartiene qualsiasi oggetto tracciato.
        if (collision.gameObject.CompareTag("Obstacle") || 
            collision.gameObject.CompareTag("Point") || 
            collision.gameObject.CompareTag("Powerup"))
        {
            MasterSpawner.Instance.ReturnObjectToPool(collision.gameObject);
        }
    }
}