using System.Collections;
using UnityEngine;

/// <summary>
/// Anima rotazione e zoom di un RectTransform (tipicamente il parent che contiene sfondo
/// + arredamento), come due fasi INDIPENDENTI cosi' puoi decidere tu l'ordine e la durata
/// di ciascuna da PlayTransitionController (es. prima zoom, poi rotazione).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SceneRotator : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("L'elemento da animare. Se vuoto, usa questo stesso RectTransform.")]
    [SerializeField] private RectTransform target;

    [Header("Rotazione")]
    [Tooltip("Angolo finale (Z) in gradi, relativo alla rotazione di partenza. 90 = verticale.")]
    [SerializeField] private float targetAngleZ = 45f;

    [Tooltip("Se true, compensa lo spostamento causato da un pivot non centrato, così la rotazione appare centrata sul suo centro geometrico invece che sul pivot.")]
    [SerializeField] private bool rotateAroundCenter = true;

    [Tooltip("Curva di easing per la rotazione (0-1 nel tempo -> 0-1 di avanzamento).")]
    [SerializeField] private AnimationCurve rotationEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Zoom / Scala")]
    [Tooltip("Scala finale, come moltiplicatore della scala di partenza AL MOMENTO in cui parte ZoomTo(). 1 = nessun cambiamento, 1.2 = +20% zoom, 0.8 = -20%.")]
    [SerializeField] private float zoomMultiplier = 1.5f;

    [Tooltip("Se true, compensa lo spostamento causato da un pivot non centrato, così lo zoom cresce/si restringe dal suo centro geometrico invece che dal pivot.")]
    [SerializeField] private bool zoomAroundCenter = true;

    [Tooltip("Curva di easing per lo zoom (0-1 nel tempo -> 0-1 di avanzamento).")]
    [SerializeField] private AnimationCurve zoomEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// Anima SOLO lo zoom (scala), verso zoomMultiplier applicato alla scala attuale.
    /// Chiamala prima di RotateTo() se vuoi che lo zoom avvenga per primo.
    /// </summary>
    public IEnumerator ZoomTo(float duration)
    {
        Vector3 startScale = target.localScale;
        Vector3 endScale = startScale * zoomMultiplier;

        float t = 0f;
        while (t < duration)
        {
            float rawT = duration > 0f ? t / duration : 1f;
            float easedT = zoomEasing.Evaluate(rawT);

            Vector3 centerBefore = zoomAroundCenter ? target.TransformPoint(target.rect.center) : Vector3.zero;

            target.localScale = Vector3.Lerp(startScale, endScale, easedT);

            if (zoomAroundCenter)
            {
                Vector3 centerAfter = target.TransformPoint(target.rect.center);
                target.position += centerBefore - centerAfter;
            }

            t += Time.deltaTime;
            yield return null;
        }

        target.localScale = endScale;
    }

    /// <summary>
    /// Anima SOLO la rotazione, verso targetAngleZ applicato alla rotazione attuale.
    /// Chiamala dopo ZoomTo() (o prima, o da sola) a seconda dell'ordine che vuoi in PlayTransitionController.
    /// </summary>
    public IEnumerator RotateTo(float duration)
    {
        Quaternion startRotation = target.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, targetAngleZ);

        float t = 0f;
        while (t < duration)
        {
            float rawT = duration > 0f ? t / duration : 1f;
            float easedT = rotationEasing.Evaluate(rawT);

            Vector3 centerBefore = rotateAroundCenter ? target.TransformPoint(target.rect.center) : Vector3.zero;

            target.rotation = Quaternion.Slerp(startRotation, endRotation, easedT);

            if (rotateAroundCenter)
            {
                Vector3 centerAfter = target.TransformPoint(target.rect.center);
                target.position += centerBefore - centerAfter;
            }

            t += Time.deltaTime;
            yield return null;
        }

        target.rotation = endRotation;
    }

    /// <summary>
    /// Utile per resettare tutto allo stato originale (es. tornando al menu).
    /// </summary>
    public void ResetTransform()
    {
        target.rotation = Quaternion.identity;
        target.localScale = Vector3.one;
    }
}