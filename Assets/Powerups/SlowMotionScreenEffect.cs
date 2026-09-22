using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Necessario per interagire con l'UI di Unity

public class SlowMotionScreenEffect : MonoBehaviour
{
    public static SlowMotionScreenEffect Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Assegna qui l'Image dell'UI che funge da overlay.")]
    public Image overlayImage;

    [Header("Effect Settings")]
    [Tooltip("Opacità massima (alpha) dell'alone azzurro.")]
    [Range(0f, 1f)] public float maxAlpha = 0.5f;
    [Tooltip("Quanto tempo impiega l'alone ad apparire e scomparire (in secondi reali).")]
    public float fadeDuration = 0.25f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        // Setup del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Assicurati che all'avvio l'alone sia invisibile
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
            overlayImage.gameObject.SetActive(false);
        }
    }

    // Metodo pubblico chiamato dal powerup
    public void ShowEffect(float duration)
    {
        if (overlayImage == null) return;
        
        // Se c'è già un fade in corso (es. il player raccoglie due powerup di fila), resettalo
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
            // Usiamo Time.unscaledDeltaTime perché il gioco è in slow motion (Time.timeScale alterato)
            time += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, time / fadeDuration);
            overlayImage.color = c;
            yield return null; // Aspetta il prossimo frame
        }

        // 2. ATTESA
        // Sottraiamo il tempo di fade per calcolare l'esatta permanenza a schermo
        float waitTime = Mathf.Max(0, duration - (fadeDuration * 2));
        yield return new WaitForSecondsRealtime(waitTime);

        // 3. FADE OUT
        time = 0;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, time / fadeDuration);
            overlayImage.color = c;
            yield return null;
        }

        // Resetta i valori e spegni il GameObject per ottimizzare le performance
        c.a = 0f;
        overlayImage.color = c;
        overlayImage.gameObject.SetActive(false);
    }
}