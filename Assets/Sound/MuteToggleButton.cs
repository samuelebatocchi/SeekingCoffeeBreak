using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Bottone globale di mute/unmute. Va su un GameObject separato (es. il
/// bottoncino speaker nel pannello Settings) e può controllare una o più
/// righe SquareVolumeControl insieme (es. Musica + Effetti con un solo click).
///
/// Quando disattivato: tutte le righe assegnate vengono silenziate (i loro
/// quadratini diventano grigi) ma il livello scelto resta salvato.
/// Quando riattivato: ogni riga torna esattamente all'ultimo livello impostato.
/// </summary>
public class MuteToggleButton : MonoBehaviour
{
    [Header("Righe da silenziare (una o più)")]
    [SerializeField] private SquareVolumeControl[] targets;

    [Header("Sprite del bottone")]
    [SerializeField] private Image buttonImage;
    [Tooltip("Sprite mostrato quando l'audio è attivo")]
    [SerializeField] private Sprite audioOnSprite;
    [Tooltip("Sprite mostrato quando l'audio è disattivato (mute)")]
    [SerializeField] private Sprite audioOffSprite;

    [Header("Salvataggio")]
    [SerializeField] private string playerPrefsKey = "GlobalMuted";

    private bool isMuted;

    private void Start()
    {
        AddClickListener(gameObject, ToggleMute);

        isMuted = PlayerPrefs.GetInt(playerPrefsKey, 0) == 1;
        ApplyState();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyState();
        PlayerPrefs.SetInt(playerPrefsKey, isMuted ? 1 : 0);
    }

    private void ApplyState()
    {
        foreach (var target in targets)
        {
            if (target != null) target.SetMuted(isMuted);
        }

        if (buttonImage != null)
        {
            buttonImage.sprite = isMuted ? audioOffSprite : audioOnSprite;
        }
    }

    // --- utility per aggiungere un click senza bisogno di un componente Button ---
    private void AddClickListener(GameObject target, System.Action onClick)
    {
        var trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((_) => onClick());
        trigger.triggers.Add(entry);
    }
}
