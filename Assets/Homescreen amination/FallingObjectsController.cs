using System.Collections;
using UnityEngine;

/// <summary>
/// Coordina la caduta "a catena" di tutti gli oggetti della scena (scrivania, monitor,
/// pianta, lampada, icone varie). Ogni oggetto nell'array deve avere un componente
/// FallableObject (Rigidbody2D + Collider2D).
///
/// Nota: se questi oggetti vivono dentro un Canvas, la fisica 2D funziona solo con
/// Canvas in modalita' "Screen Space - Camera" o "World Space" con una Camera 2D dietro.
/// Con "Screen Space - Overlay" il Rigidbody2D non ha effetto: in quel caso valuta di
/// spostare questi oggetti fuori dal Canvas (world space puro) solo per questa scena/fase.
/// </summary>
public class FallingObjectsController : MonoBehaviour
{
    [Tooltip("Tutti gli oggetti che devono cadere, nell'ordine in cui vuoi che partano.")]
    [SerializeField] private FallableObject[] fallableObjects;

    [Tooltip("Ritardo tra la caduta di un oggetto e il successivo, per un effetto 'a cascata'.")]
    [SerializeField] private float delayBetweenDrops = 0.05f;

    [Tooltip("Tempo di attesa dopo l'ultima caduta, per lasciare che gli oggetti atterrino/escano dallo schermo.")]
    [SerializeField] private float settleTime = 1f;

    public IEnumerator DropAll()
    {
        foreach (var obj in fallableObjects)
        {
            if (obj != null)
            {
                obj.Drop();
            }
            yield return new WaitForSeconds(delayBetweenDrops);
        }

        yield return new WaitForSeconds(settleTime);
    }
}
