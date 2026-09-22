using System.Collections;
using UnityEngine;

/// <summary>
/// Fa tremare uno RectTransform (tipicamente HomePanel). A differenza della versione
/// precedente, ora lo shake e' CONTINUO: parte con StartShaking() e resta attivo finche'
/// non chiami StopShaking(), cosi' puoi farlo sovrapporre ad altre animazioni (rotazione,
/// caduta oggetti, corsa del personaggio) invece di essere legato a una durata fissa.
/// </summary>
public class ScreenShaker : MonoBehaviour
{
    [Tooltip("L'elemento da far tremare. Se vuoto, usa questo stesso RectTransform.")]
    [SerializeField] private RectTransform target;

    [Tooltip("Intensita' dello shake (pixel). Puoi anche passarla via codice con StartShaking(magnitude).")]
    [SerializeField] private float magnitude = 10f;

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;
    private bool isShaking;

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// Avvia lo shake continuo con l'intensita' impostata nell'Inspector.
    /// </summary>
    public void StartShaking()
    {
        if (isShaking) return;

        originalLocalPos = target.localPosition;
        isShaking = true;
        shakeRoutine = StartCoroutine(ShakeLoop());
    }

    /// <summary>
    /// Avvia lo shake continuo con un'intensita' specifica.
    /// </summary>
    public void StartShaking(float shakeMagnitude)
    {
        magnitude = shakeMagnitude;
        StartShaking();
    }

    /// <summary>
    /// Ferma lo shake e riporta l'oggetto alla posizione originale.
    /// </summary>
    public void StopShaking()
    {
        if (!isShaking) return;

        isShaking = false;
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }
        target.localPosition = originalLocalPos;
    }

    private IEnumerator ShakeLoop()
    {
        while (isShaking)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            target.localPosition = originalLocalPos + new Vector3(x, y, 0f);
            yield return null;
        }
    }

    /// <summary>
    /// Compatibilita' con l'uso precedente: shake per una durata fissa, poi si ferma da solo.
    /// Utile se in qualche punto ti serve ancora uno shake "a tempo" invece che manuale.
    /// </summary>
    public IEnumerator Shake(float duration, float shakeMagnitude)
    {
        StartShaking(shakeMagnitude);
        yield return new WaitForSeconds(duration);
        StopShaking();
    }
}