using System.Collections;
using UnityEngine;

public class PowerupItem_slowmotion : PowerupItem
{
    [Header("Impostazioni Slow Motion")]
    [Tooltip("Fattore di scala del tempo. 0.5 = il gioco va alla metà della velocità.")]
    [SerializeField] [Range(0.1f, 1f)] private float timeScaleMultiplier = 0.5f;
    
    [Tooltip("Durata dell'effetto in secondi (basato sul tempo reale).")]
    [SerializeField] private float effectDurationInSeconds = 3f;

    public override void Activate(Character player)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlaySlowMotionSound();

        player.StartCoroutine(SlowMotionRoutine());
    }

    private IEnumerator SlowMotionRoutine()
    {
        if (Time.timeScale < 1f)
        {
            yield break;
        }

        // --- INIZIO MODIFICA: Richiamo l'UI Overlay ---
        if (SlowMotionScreenEffect.Instance != null)
        {
            SlowMotionScreenEffect.Instance.ShowEffect(effectDurationInSeconds);
        }
        // --- FINE MODIFICA ---

        float defaultFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = timeScaleMultiplier;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        yield return new WaitForSecondsRealtime(effectDurationInSeconds);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}