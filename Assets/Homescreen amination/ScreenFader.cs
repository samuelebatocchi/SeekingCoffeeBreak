using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestore persistente delle dissolvenze tra scene (fade to black in uscita, fade in in entrata).
///
/// Va creato UNA SOLA VOLTA, nella scena del menu, su un GameObject dedicato (es. "ScreenFader")
/// con un Canvas (Render Mode: Screen Space - Overlay, Sort Order molto alto, es. 999) che
/// contiene un'Image nera a schermo intero, e un CanvasGroup sullo stesso GameObject di questo
/// script per controllarne l'opacita'.
///
/// Grazie a DontDestroyOnLoad, lo stesso pannello nero sopravvive al cambio scena: fa la
/// dissolvenza in USCITA dal menu, resta nero durante il caricamento, e fa la dissolvenza in
/// ENTRATA appena la scena di gioco e' pronta. Non serve duplicare nulla in MainScene.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Singleton: se esiste gia' un'istanza (es. per un click doppio o un reload), distruggi il duplicato.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>
    /// Fa la dissolvenza al nero, carica la scena indicata (in modo asincrono), poi fa
    /// la dissolvenza in entrata sulla nuova scena.
    /// </summary>
    public void LoadSceneWithFade(string sceneName, float fadeOutDuration, float fadeInDuration)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, fadeOutDuration, fadeInDuration));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, float fadeOutDuration, float fadeInDuration)
    {
        yield return StartCoroutine(FadeOut(fadeOutDuration));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(FadeIn(fadeInDuration));
    }

    public IEnumerator FadeOut(float duration)
    {
        canvasGroup.blocksRaycasts = true;
        yield return Fade(canvasGroup.alpha, 1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(canvasGroup.alpha, 0f, duration);
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, duration > 0f ? t / duration : 1f);
            t += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
