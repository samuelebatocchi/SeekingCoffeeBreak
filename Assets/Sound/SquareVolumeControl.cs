using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

/// <summary>
/// Controllo volume "a quadratini" (come in molti giochi retro/pixel).
/// Assegna questo script a un GameObject vuoto dentro il tuo pannello Settings,
/// uno per ogni riga (uno per "Volume" musica, uno per "Musica"/SFX, ecc).
///
/// Ogni quadratino è una Image; il click viene gestito tramite EventTrigger,
/// quindi non serve un componente Button separato su ogni quadratino.
///
/// Il mute NON è gestito qui: questo script espone SetMuted()/IsMuted così
/// un componente esterno (es. MuteToggleButton, su un altro GameObject) può
/// silenziare una o più righe con un unico bottone.
/// </summary>
public class SquareVolumeControl : MonoBehaviour
{
    [Header("Quadratini (in ordine da sinistra a destra)")]
    [SerializeField] private Image[] squares;

    [Header("Sprite quadratini")]
    [SerializeField] private Sprite squareOnSprite;   // il tuo asset giallo
    [SerializeField] private Sprite squareOffSprite;  // il tuo asset grigio

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [Tooltip("Nome esatto del parametro esposto nel mixer, es. 'MusicVolume' o 'SFXVolume'")]
    [SerializeField] private string exposedParameterName = "MusicVolume";

    [Header("Salvataggio")]
    [Tooltip("Chiave usata per salvare il livello in PlayerPrefs")]
    [SerializeField] private string playerPrefsKey = "MusicVolumeLevel";

    [Header("Range in dB del mixer (di solito va bene così)")]
    [SerializeField] private float minDB = -80f;
    [SerializeField] private float maxDB = 0f;

    private int currentLevel;  // ultimo livello scelto (0..squares.Length), indipendente dal mute
    private bool isMuted;

    /// <summary>True se questa riga è attualmente silenziata (impostato da fuori, es. MuteToggleButton).</summary>
    public bool IsMuted => isMuted;

    private void Start()
    {
        // Click sui quadratini
        for (int i = 0; i < squares.Length; i++)
        {
            int index = i; // cattura la variabile per la lambda
            AddClickListener(squares[i].gameObject, () => SetLevel(index + 1));
        }

        // Carica lo stato salvato (default: tutti accesi)
        currentLevel = PlayerPrefs.GetInt(playerPrefsKey, squares.Length);

        Refresh();
    }

    /// <summary>
    /// Imposta quanti quadratini sono accesi (1 = solo il primo, squares.Length = tutti).
    /// Selezionare un livello direttamente riattiva l'audio se era in mute.
    /// </summary>
    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, squares.Length);
        isMuted = false; // scegliere un livello con i quadratini riattiva sempre il suono

        Refresh();
        PlayerPrefs.SetInt(playerPrefsKey, currentLevel);
    }

    /// <summary>
    /// Silenzia o riattiva questa riga senza toccare il livello salvato:
    /// richiamato da un bottone mute esterno (es. MuteToggleButton).
    /// </summary>
    public void SetMuted(bool muted)
    {
        isMuted = muted;
        Refresh();
    }

    private void Refresh()
    {
        int displayLevel = isMuted ? 0 : currentLevel;

        // Aggiorna gli sprite dei quadratini (tutti grigi se muto)
        for (int i = 0; i < squares.Length; i++)
        {
            squares[i].sprite = (i < displayLevel) ? squareOnSprite : squareOffSprite;
        }

        // Applica il volume reale al mixer
        float normalizedVolume = (!isMuted && squares.Length > 0) ? (float)currentLevel / squares.Length : 0f;
        ApplyVolume(normalizedVolume);
    }

    private void ApplyVolume(float normalizedVolume)
    {
        if (audioMixer == null) return;

        float dB;
        if (normalizedVolume <= 0.0001f)
        {
            dB = minDB; // muto
        }
        else
        {
            // scala logaritmica: il mixer lavora in dB, l'orecchio umano in scala log
            dB = Mathf.Log10(normalizedVolume) * 20f;
            dB = Mathf.Clamp(dB, minDB, maxDB);
        }

        audioMixer.SetFloat(exposedParameterName, dB);
    }

    // --- utility per aggiungere un click a una Image senza bisogno di un componente Button ---
    private void AddClickListener(GameObject target, System.Action onClick)
    {
        var trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((_) => onClick());
        trigger.triggers.Add(entry);
    }
}