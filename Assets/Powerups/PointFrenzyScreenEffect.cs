using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PointFrenzyScreenEffect : MonoBehaviour
{
    public static PointFrenzyScreenEffect Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Assegna qui l'Image dell'UI che funge da overlay per il Point Frenzy.")]
    public Image overlayImage;

    [Header("Effect Settings")]
    [Tooltip("Opacità massima (alpha) dell'alone giallo.")]
    [Range(0f, 1f)] public float maxAlpha = 0.5f;
    [Tooltip("Quanto tempo impiega l'alone ad apparire e scomparire (in secondi).")]
    public float fadeDuration = 0.25f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        // Setup del Singleton per l'accesso globale
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Inizializza l'overlay come invisibile e disattivato
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
            overlayImage.gameObject.SetActive(false);
        }
    }

    public void ShowEffect(float duration)
    {
        if (overlayImage == null) return;
        
        // Interrompe eventuali Coroutine precedenti per evitare sovrapposizioni visive
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        
        fadeRoutine = StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        overlayImage.gameObject.SetActive(true);
        Color c = overlayImage.color;

        // 1. FADE IN
        float time = 0;
        while (time < fadeDuration)
        {
            // Utilizzo Time.deltaTime standard poiché il timeScale non è alterato
            time += Time.deltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, time / fadeDuration);
            overlayImage.color = c;
            yield return null; 
        }

        // 2. ATTESA
        // Calcola il tempo di permanenza sottraendo le durate del fade in e fade out
        float waitTime = Mathf.Max(0, duration - (fadeDuration * 2));
        yield return new WaitForSeconds(waitTime);

        // 3. FADE OUT
        time = 0;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, time / fadeDuration);
            overlayImage.color = c;
            yield return null;
        }

        // Disattiva l'Image per non impattare le performance (Overdraw)
        c.a = 0f;
        overlayImage.color = c;
        overlayImage.gameObject.SetActive(false);
    }
}