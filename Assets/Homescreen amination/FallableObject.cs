using System.Collections;
using UnityEngine;

/// <summary>
/// Da mettere sui singoli oggetti UI che devono "cadere" (scrivania, monitor, pianta,
/// lampada, icone...). Funziona su elementi dentro un Canvas (anche Screen Space -
/// Overlay), perche' NON usa la fisica reale: simula la caduta muovendo direttamente
/// anchoredPosition e ruotando il RectTransform, frame per frame.
///
/// Ogni oggetto puo' avere parametri diversi (gravita', rotazione, direzione) cosi'
/// non cadono tutti allo stesso modo.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FallableObject : MonoBehaviour
{
    [Header("Direzione di caduta")]
    [Tooltip("Direzione principale della caduta (non serve normalizzarla). (0,-1) = dritto verso il basso, (-1,-1) = in basso a sinistra, (1,-1) = in basso a destra, (-1,0) = tutto a sinistra, ecc.")]
    [SerializeField] private Vector2 fallDirection = new Vector2(0f, -1f);

    [Tooltip("Variazione casuale intorno alla direzione principale, in gradi. 0 = tutti gli oggetti cadono esattamente nella stessa direzione. Valori tipici: 10-30.")]
    [SerializeField] private float directionRandomness = 15f;

    [Header("Gravita' e rotazione")]
    [Tooltip("Accelerazione di caduta lungo la direzione scelta (pixel/secondo^2). Valori piu' alti = caduta piu' rapida.")]
    [SerializeField] private Vector2 gravityRange = new Vector2(1200f, 2200f);

    [Tooltip("Velocita' angolare di rotazione durante la caduta (gradi/secondo). Il segno viene randomizzato.")]
    [SerializeField] private Vector2 angularSpeedRange = new Vector2(60f, 220f);

    [Tooltip("Intensita' della spinta iniziale lungo la direzione di caduta (pixel/secondo).")]
    [SerializeField] private float kickStrength = 150f;

    [Header("Quando fermarsi")]
    [Tooltip("Se true, l'oggetto viene disattivato quando esce abbastanza dallo schermo.")]
    [SerializeField] private bool disableWhenOffscreen = true;

    [Tooltip("Quanto deve allontanarsi (in pixel, in linea d'aria dalla posizione iniziale) prima di essere considerato 'fuori schermo'.")]
    [SerializeField] private float offscreenDropDistance = 2000f;

    [Tooltip("Tempo massimo di caduta, come sicurezza nel caso non esca mai dai limiti sopra.")]
    [SerializeField] private float maxFallTime = 3f;

    private RectTransform rt;
    private bool isFalling;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Avvia la caduta simulata. Puoi chiamarlo direttamente (parte in autonomia)
    /// oppure aspettarne la coroutine se vuoi sincronizzarti con altro.
    /// </summary>
    public void Drop()
    {
        if (isFalling) return;
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        isFalling = true;

        // Ruota la direzione base di un angolo casuale entro +/- directionRandomness/2,
        // cosi' gli oggetti non cadono tutti esattamente nella stessa identica linea.
        float randomAngleOffset = Random.Range(-directionRandomness, directionRandomness) * 0.5f;
        Vector2 direction = RotateVector(fallDirection.normalized, randomAngleOffset);

        float gravity = Random.Range(gravityRange.x, gravityRange.y);
        float angularSpeed = Random.Range(angularSpeedRange.x, angularSpeedRange.y) * (Random.value < 0.5f ? -1f : 1f);

        Vector2 velocity = direction * kickStrength;
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < maxFallTime)
        {
            float dt = Time.deltaTime;

            // "gravita'" lungo la direzione scelta, non necessariamente dritta verso il basso
            velocity += direction * gravity * dt;

            rt.anchoredPosition += velocity * dt;
            rt.localEulerAngles += new Vector3(0f, 0f, angularSpeed * dt);

            if (disableWhenOffscreen && Vector2.Distance(startPos, rt.anchoredPosition) > offscreenDropDistance)
            {
                break;
            }

            elapsed += dt;
            yield return null;
        }

        if (disableWhenOffscreen)
        {
            gameObject.SetActive(false);
        }

        isFalling = false;
    }

    /// <summary>
    /// Ruota un Vector2 di un certo angolo in gradi (senso antiorario positivo).
    /// </summary>
    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}