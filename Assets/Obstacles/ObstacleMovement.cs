using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] 
public class ObstacleMovement : MonoBehaviour
{
    // Variabile nascosta nell'Inspector, modificata dinamicamente dal MasterSpawner
    [HideInInspector] public float localSpeedMultiplier = 1f;

    private void OnEnable()
    {
        // Reset fondamentale per l'Object Pool: quando l'oggetto viene riattivato,
        // torna ad avere la velocità di default (moltiplicatore 1x).
        localSpeedMultiplier = 1f;
    }

    private void Update()
    {
        // Movimento basato sul frame rate, sincronizzato con i Wall[cite: 4].
        // Ora include il localSpeedMultiplier per gestire gli ostacoli più veloci.
        float moveDistance = GameManager.Instance.CurrentSpeed * localSpeedMultiplier * Time.deltaTime;
        transform.position += Vector3.down * moveDistance;
    }
}