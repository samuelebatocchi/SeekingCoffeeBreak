using UnityEngine;

/// <summary>
/// Aggiunge un effetto di pulsazione (cambio di scala) sinusoidale al GameObject.
/// Da attaccare ai prefab di Point e Powerup.
/// </summary>
public class PulsingEffect : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("La velocità con cui l'oggetto pulsa.")]
    public float pulseSpeed = 5f;

    [Tooltip("La scala minima raggiunta dall'oggetto.")]
    public float minScale = 0.8f;

    [Tooltip("La scala massima raggiunta dall'oggetto.")]
    public float maxScale = 1.2f;

    // Memorizza la scala originale (in caso il prefab non parta da 1,1,1)
    private Vector3 originalScale;
    
    // Tiene traccia del tempo indipendente per non resettarsi bruscamente
    private float pulseTimer;

    private void Awake()
    {
        // Salviamo la scala di partenza per mantenere le proporzioni originali del prefab
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Reset del timer vitale per l'Object Pool: garantisce che la pulsazione 
        // riparta pulita ogni volta che l'oggetto viene ripescato dalla pool.
        pulseTimer = 0f;
        transform.localScale = originalScale;
    }

    private void Update()
    {
        // Incrementiamo il timer interno. 
        // Usiamo un timer interno invece di Time.time perché con l'Object Pool, 
        // Time.time continuerebbe a scorrere anche quando l'oggetto è disattivato.
        pulseTimer += Time.deltaTime * pulseSpeed;

        // Mathf.Sin oscilla tra -1 e 1.
        float sinValue = Mathf.Sin(pulseTimer);

        // Rimappiamo il valore da [-1, 1] al range [0, 1] per poter usare il Lerp facilmente
        // (sinValue + 1f) porta il range a [0, 2], dividendo per 2f otteniamo [0, 1]
        float t = (sinValue + 1f) / 2f;

        // Calcoliamo il moltiplicatore di scala interpolando tra minScale e maxScale
        float scaleMultiplier = Mathf.Lerp(minScale, maxScale, t);

        // Applichiamo la nuova scala mantenendo le proporzioni originali (originalScale)
        transform.localScale = originalScale * scaleMultiplier;
    }
}