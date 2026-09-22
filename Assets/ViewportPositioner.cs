using UnityEngine;

/// Posiziona dinamicamente il GameObject lungo l'asse X in base al Viewport della Camera.

public class ViewportPositioner : MonoBehaviour
{
    [Header("Alignment Settings")]
    [Tooltip("Posizione Viewport X (0 = bordo sinistro schermo, 1 = bordo destro schermo). I Wall sono a 0.08 e 0.92.")]
    [Range(0f, 1f)]
    public float viewportXTarget = 0.5f;

    // Utilizzo Awake invece di Start per garantire che i Transform siano 
    // posizionati PRIMA che gli Spawner inizino a spawnare oggetti in Start/Update
    private void Awake()
    {
        PositionRelativeToViewport();
    }

    private void PositionRelativeToViewport()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera non trovata!");
            return;
        }

        // Calcola la distanza Z dalla camera per la corretta proiezione
        float zDistance = Mathf.Abs(cam.transform.position.z);

        // Converte le coordinate Viewport in World Space.
        Vector3 calculatedWorldPos = cam.ViewportToWorldPoint(new Vector3(viewportXTarget, 0f, zDistance));

        // Aggiorna la posizione del Transform.
        transform.position = new Vector3(calculatedWorldPos.x, transform.position.y, transform.position.z);
    }
}