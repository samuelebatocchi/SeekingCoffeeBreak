using System.Collections;
using UnityEngine;

/// <summary>
/// "Regista" della sequenza che parte quando si preme PLAY:
/// lo shake parte SUBITO e resta attivo per tutta la sequenza; zoom e rotazione partono
/// ciascuno dopo il proprio "Start Delay"; poi caduta oggetti; nel momento indicato da
/// Character Switch Delay, l'animazione del personaggio viene sostituita con quella
/// indicata in Character Animation State; poi lo shake si ferma e parte la dissolvenza
/// al nero seguita dal caricamento della scena di gioco con dissolvenza in entrata.
/// </summary>
public class PlayTransitionController : MonoBehaviour
{
    [Header("Componenti effetto")]
    [SerializeField] private ScreenShaker screenShaker;
    [SerializeField] private SceneRotator sceneRotator;
    [SerializeField] private FallingObjectsController objectDropper;
    [SerializeField] private WorkerCharacterController characterController;
    [SerializeField] private ScreenFader screenFader;

    [Header("Shake (continuo dall'inizio alla fine dell'azione)")]
    [SerializeField] private float shakeMagnitude = 12f;

    [Header("Zoom")]
    [Tooltip("Quanto aspettare (in secondi, dall'inizio della sequenza) prima che parta lo zoom.")]
    [SerializeField] private float zoomStartDelay = 0f;
    [SerializeField] private float zoomDuration = 1f;

    [Header("Rotazione")]
    [Tooltip("Quanto aspettare (in secondi, dall'inizio della sequenza) prima che parta la rotazione.")]
    [SerializeField] private float rotateStartDelay = 1f;
    [SerializeField] private float rotateDuration = 1.8f;

    [Header("Caduta oggetti")]
    [Tooltip("Quanto aspettare (in secondi, dall'inizio della sequenza) prima che gli oggetti comincino a cadere.")]
    [SerializeField] private float objectDropStartDelay = 0.5f;

    [Header("Personaggio")]
    [Tooltip("Trascina qui l'Animator Controller a cui passare (es. 'Worker Transition').")]
    [SerializeField] private RuntimeAnimatorController characterAnimatorController;
    [Tooltip("Nuova Scale X/Y del personaggio (lo stesso campo 'Scale' che vedi sul RectTransform nell'Inspector) nel momento dello switch. Controlla il valore attuale di Scale sull'oggetto prima di decidere questi numeri: nel tuo caso probabilmente era intorno a 70/70.")]
    [SerializeField] private Vector2 characterTargetSize = new Vector2(70f, 70f);
    [Tooltip("Nuova posizione del personaggio (anchoredPosition, relativa al parent 'worker') nel momento dello switch. Dato che il personaggio e' annidato dentro l'oggetto che ruota, questa posizione segue automaticamente l'inclinazione del terreno.")]
    [SerializeField] private Vector2 characterTargetPosition = Vector2.zero;
    [Tooltip("Quanto aspettare (in secondi, dall'inizio della sequenza) prima di sostituire Animator Controller, dimensioni e posizione del personaggio.")]
    [SerializeField] private float characterSwitchDelay = 2.5f;

    [Header("Personaggio - secondo spostamento (indipendente dal primo)")]
    [Tooltip("Nuova posizione (anchoredPosition) a cui spostare il personaggio in un secondo momento, separato dal cambio sopra.")]
    [SerializeField] private Vector2 characterSecondPosition = Vector2.zero;
    [Tooltip("Sprite fisso da mostrare in questo stesso istante, al posto dell'animazione (l'Animator viene disattivato). Lascia vuoto se non vuoi congelare l'immagine qui.")]
    [SerializeField] private Sprite characterStaticSprite;
    [Tooltip("Quanto aspettare (in secondi, dall'inizio della sequenza) prima di questo secondo spostamento.")]
    [SerializeField] private float characterSecondMoveDelay = 3.5f;

    [Header("Dissolvenza")]
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private float fadeInDuration = 0.6f;

    [Header("Scena successiva")]
    [Tooltip("Nome esatto della scena di gioco (deve essere in Build Settings).")]
    [SerializeField] private string nextSceneName;

    private bool isPlaying;

    /// <summary>
    /// Collega questo metodo all'OnClick del PlayButton al posto del caricamento diretto della scena.
    /// </summary>
    public void OnPlayPressed()
    {
        if (isPlaying) return;
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        isPlaying = true;
        MusicManager.Instance.StopMusic();
        MusicManager.Instance.PlayTransitionSound();
        // Lo shake parte subito e resta attivo in sottofondo per tutta la sequenza.
        screenShaker.StartShaking(shakeMagnitude);

        // Zoom, rotazione e cambio animazione del personaggio partono ciascuno con il
        // proprio ritardo, tutti in parallelo tra loro.
        Coroutine zoomRoutine = StartCoroutine(DelayedZoom());
        Coroutine rotateRoutine = StartCoroutine(DelayedRotate());
        Coroutine characterRoutine = StartCoroutine(DelayedCharacterSwitch());
        Coroutine characterMoveRoutine = StartCoroutine(DelayedCharacterMove());
        Coroutine dropRoutine = StartCoroutine(DelayedObjectDrop());

        yield return zoomRoutine;
        yield return rotateRoutine;
        yield return characterRoutine;
        yield return characterMoveRoutine;
        yield return dropRoutine;

        // L'azione principale e' finita: fermiamo lo shake prima della dissolvenza al nero.
        screenShaker.StopShaking();
        MusicManager.Instance.StopTransitionSound();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (screenFader != null)
            {
                screenFader.LoadSceneWithFade(nextSceneName, fadeOutDuration, fadeInDuration);
            }
            else
            {
                // Fallback se non hai collegato uno ScreenFader: cambio scena diretto, senza dissolvenza.
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }

        isPlaying = false;
    }

    private IEnumerator DelayedZoom()
    {
        if (zoomStartDelay > 0f)
        {
            yield return new WaitForSeconds(zoomStartDelay);
        }
        yield return StartCoroutine(sceneRotator.ZoomTo(zoomDuration));
    }

    private IEnumerator DelayedRotate()
    {
        if (rotateStartDelay > 0f)
        {
            yield return new WaitForSeconds(rotateStartDelay);
        }
        yield return StartCoroutine(sceneRotator.RotateTo(rotateDuration));
    }

    private IEnumerator DelayedCharacterSwitch()
    {
        if (characterSwitchDelay > 0f)
        {
            yield return new WaitForSeconds(characterSwitchDelay);
        }
        characterController.SwitchAppearance(characterAnimatorController, characterTargetSize, characterTargetPosition);
    }

    private IEnumerator DelayedCharacterMove()
    {
        if (characterSecondMoveDelay > 0f)
        {
            yield return new WaitForSeconds(characterSecondMoveDelay);
        }
        characterController.SetPosition(characterSecondPosition);

        if (characterStaticSprite != null)
        {
            characterController.SetStaticImage(characterStaticSprite);
        }
    }

    private IEnumerator DelayedObjectDrop()
    {
        if (objectDropStartDelay > 0f)
        {
            yield return new WaitForSeconds(objectDropStartDelay);
        }
        yield return StartCoroutine(objectDropper.DropAll());
    }
}