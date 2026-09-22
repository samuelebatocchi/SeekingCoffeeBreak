using UnityEngine;

// Questo script va assegnato ai Prefab dei Point nell'Inspector.
public class PointItem : MonoBehaviour
{
    [Header("Point Settings")]
    [Tooltip("Valore che verrà aggiunto allo Score quando il Character raccoglie questo oggetto.")]
    public int scoreValue = 0; // Modifica questo valore nell'Inspector per ogni diverso Prefab
}