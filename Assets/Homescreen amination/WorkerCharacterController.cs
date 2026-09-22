using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wrapper minimale per il personaggio: nessuna logica su COSA deve succedere e QUANDO,
/// solo metodi per sostituire l'Animator Controller in uso e/o cambiare dimensioni e
/// posizione del personaggio, da chiamare nel momento esatto in cui vuoi tu.
///
/// La posizione e' espressa in anchoredPosition, cioe' RELATIVA al parent (worker/Background):
/// dato che il personaggio e' annidato dentro l'oggetto che ruota, non serve alcun calcolo
/// per "seguire" l'inclinazione del terreno quando lo sfondo e' ruotato - viene gia' cosi'.
/// </summary>
public class WorkerCharacterController : MonoBehaviour
{
    [Tooltip("L'Animator del personaggio. Se vuoto, prova a prenderlo da questo stesso GameObject.")]
    [SerializeField] private Animator characterAnimator;

    [Tooltip("Il RectTransform del personaggio, per cambiarne dimensioni/posizione. Se vuoto, usa questo stesso RectTransform.")]
    [SerializeField] private RectTransform rectTransform;

    [Tooltip("Il componente Image del personaggio (quello con 'Source Image'), per mostrare uno sprite fisso. Se vuoto, prova a prenderlo da questo stesso GameObject.")]
    [SerializeField] private Image characterImage;

    private void Awake()
    {
        if (characterAnimator == null)
        {
            characterAnimator = GetComponent<Animator>();
        }

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (characterImage == null)
        {
            characterImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Sostituisce l'INTERO Animator Controller in uso con un altro (trascinalo direttamente
    /// nell'Inspector, es. l'asset "Worker Transition"). L'Animator parte automaticamente
    /// dallo stato di default del nuovo Controller.
    /// </summary>
    public void SwitchController(RuntimeAnimatorController newController)
    {
        if (newController == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessun Animator Controller assegnato.");
            return;
        }

        if (characterAnimator == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessun Animator assegnato.");
            return;
        }

        characterAnimator.runtimeAnimatorController = newController;
    }

    /// <summary>
    /// Cambia proporzioni (Scale X/Y, lo stesso campo "Scale" che vedi nell'Inspector del
    /// RectTransform) e posizione del personaggio, istantaneamente.
    ///
    /// NOTA: qui uso localScale invece di sizeDelta di proposito. Se gli anchor del
    /// RectTransform sono "stretchati" (min e max diversi su un asse, invece che un punto
    /// fisso), sizeDelta non e' la dimensione assoluta ma solo uno scostamento aggiunto a
    /// una dimensione calcolata dal parent - per questo cambiarlo di poco non si vedeva.
    /// La Scale invece funziona sempre in modo prevedibile, qualunque sia la configurazione
    /// degli anchor.
    ///
    /// La posizione e' anchoredPosition, cioe' relativa al parent - dato che il personaggio
    /// e' annidato dentro l'oggetto che ruota (Background), risultera' gia' orientata
    /// correttamente rispetto al "terreno" inclinato senza bisogno di calcoli aggiuntivi.
    /// </summary>
    public void SetAppearance(Vector2 newScale, Vector2 newAnchoredPosition)
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessun RectTransform assegnato.");
            return;
        }

        rectTransform.localScale = new Vector3(newScale.x, newScale.y, rectTransform.localScale.z);
        rectTransform.anchoredPosition = newAnchoredPosition;
    }

    /// <summary>
    /// Comodo per fare tutto insieme in un'unica chiamata: cambio Controller + scala + posizione.
    /// </summary>
    public void SwitchAppearance(RuntimeAnimatorController newController, Vector2 newScale, Vector2 newAnchoredPosition)
    {
        SwitchController(newController);
        SetAppearance(newScale, newAnchoredPosition);
    }

    /// <summary>
    /// Cambia SOLO la posizione del personaggio (anchoredPosition), senza toccare scala o
    /// Animator Controller. Utile per spostarlo di nuovo in un secondo momento, indipendente
    /// da quando cambi aspetto con SetAppearance()/SwitchAppearance().
    /// </summary>
    public void SetPosition(Vector2 newAnchoredPosition)
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessun RectTransform assegnato.");
            return;
        }

        rectTransform.anchoredPosition = newAnchoredPosition;
    }

    /// <summary>
    /// Sostituisce l'animazione attuale con uno stato all'INTERNO dello stesso Controller
    /// (utile solo se le due animazioni vivono nello stesso Animator Controller).
    /// </summary>
    public void SwitchAnimation(string stateName, float crossFadeDuration = 0.1f)
    {
        if (characterAnimator == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessun Animator assegnato.");
            return;
        }

        characterAnimator.CrossFade(stateName, crossFadeDuration);
    }

    /// <summary>
    /// Alternativa via trigger, se preferisci gestire tu le transizioni con un trigger
    /// nell'Animator Controller invece di passare il nome dello stato.
    /// </summary>
    public void TriggerAnimation(string triggerName)
    {
        if (characterAnimator == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessun Animator assegnato.");
            return;
        }

        characterAnimator.SetTrigger(triggerName);
    }

    /// <summary>
    /// "Congela" il personaggio su un'immagine fissa: disattiva l'Animator (cosi' smette
    /// di sovrascrivere lo sprite ogni frame) e imposta direttamente lo sprite indicato
    /// sul componente Image. Utile per fermare l'animazione in un istante preciso e mostrare
    /// una posa statica invece di continuare a riprodurre l'Animator Controller attuale.
    /// </summary>
    public void SetStaticImage(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogWarning("WorkerCharacterController: nessuno sprite assegnato per l'immagine fissa.");
            return;
        }

        if (characterAnimator != null)
        {
            characterAnimator.enabled = false;
        }

        if (characterImage != null)
        {
            characterImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("WorkerCharacterController: nessun componente Image assegnato.");
        }
    }

    /// <summary>
    /// Riattiva l'Animator dopo un eventuale SetStaticImage(), se in futuro ti serve
    /// far ripartire l'animazione invece di restare sull'immagine fissa.
    /// </summary>
    public void ResumeAnimator()
    {
        if (characterAnimator != null)
        {
            characterAnimator.enabled = true;
        }
    }
}