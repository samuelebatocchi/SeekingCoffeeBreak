using UnityEngine;
using System.Collections;

public class PowerupItem_PointFrenzy : PowerupItem
{
    [Header("Point Frenzy Settings")]
    [Tooltip("Durata in secondi per cui gli spawner genereranno solo punti.")]
    [SerializeField] private float durationInSeconds = 5f;

    [Tooltip("Timer tra uno spawn e l'altro DURANTE il Frenzy (es. ogni secondo).")]
    [SerializeField] private float frenzySpawnInterval = 1f;

    public override void Activate(Character player)
    {
        // Attiva lo spawn frenetico tramite il MasterSpawner
        if (MasterSpawner.Instance != null)
        {
            MasterSpawner.Instance.ActivatePointFrenzy(durationInSeconds, frenzySpawnInterval);
        }

        // --- INIZIO MODIFICA: Chiamata all'effetto UI ---
        // Richiama l'overlay visivo passando la stessa durata del powerup
        if (PointFrenzyScreenEffect.Instance != null)
        {
            PointFrenzyScreenEffect.Instance.ShowEffect(durationInSeconds);
        }
        // --- FINE MODIFICA ---
    }
}