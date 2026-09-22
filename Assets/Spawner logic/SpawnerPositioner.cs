using UnityEngine;

/// <summary>
/// Posiziona un array di Transform (spawners) in modo equidistante lungo l'asse X.
/// Mantiene gli spawner all'interno dei limiti dei Wall e li allinea sull'asse Y 
/// al di fuori del Viewport. Calcoli eseguiti in World Space per precisione assoluta.
/// </summary>
public class SpawnerPositioner : MonoBehaviour
{
    [Header("Spawners Reference")]
    [Tooltip("Array dei Transform che fungono da punti di spawn condivisi.")]
    public Transform[] spawnPoints;

    [Header("Boundaries Settings")]
    [Tooltip("Posizione Viewport X del muro sinistro (deve coincidere con WallManager).")]
    [Range(0f, 1f)] public float leftWallViewportX = 0.08f;
    
    [Tooltip("Posizione Viewport X del muro destro (deve coincidere con WallManager).")]
    [Range(0f, 1f)] public float rightWallViewportX = 0.92f;
    
    [Tooltip("Padding in World Units tra il muro e lo spawner più vicino per evitare compenetrazioni.")]
    public float worldPadding = 1f;

    [Header("Vertical Positioning")]
    [Tooltip("Distanza in World Units al di sopra del bordo superiore della camera.")]
    public float topOffset = 2f;

    // Utilizzo Awake per garantire che i Transform siano in posizione 
    // PRIMA che il MasterSpawner inizi a utilizzarli in Start/Update
    private void Awake()
    {
        PositionSpawners();
    }

    private void PositionSpawners()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera non trovata!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length < 2)
        {
            Debug.LogWarning("Assegnare almeno 2 spawner all'array spawnPoints.");
            return;
        }

        // Calcola la distanza Z dalla camera per la corretta proiezione in World Space
        float zDist = Mathf.Abs(cam.transform.position.z);

        // 1. Calcolo asse Y: Bordo superiore dello schermo in World Space + offset
        float topScreenY = cam.ViewportToWorldPoint(new Vector3(0f, 1f, zDist)).y;
        float spawnY = topScreenY + topOffset;

        // 2. Calcolo dei limiti laterali X (facce interne dei muri) in World Space
        float leftWallWorldX = cam.ViewportToWorldPoint(new Vector3(leftWallViewportX, 0f, zDist)).x;
        float rightWallWorldX = cam.ViewportToWorldPoint(new Vector3(rightWallViewportX, 0f, zDist)).x;

        // 3. Applicazione del padding in World Space per definire l'area giocabile effettiva
        // Questo garantisce che lo spazio tra muro e oggetto sia costante su ogni aspect ratio
        float startX = leftWallWorldX + worldPadding;
        float endX = rightWallWorldX - worldPadding;

        // 4. Calcolo dello step (spazio) perfetto tra ogni spawner
        float totalUsableWidth = endX - startX;
        float stepX = totalUsableWidth / (spawnPoints.Length - 1);

        // 5. Posizionamento e allineamento definitivo dei Transform
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null) continue;

            // X = punto di partenza + (spazio * indice) per garantire l'equidistanza
            float currentX = startX + (stepX * i);
            
            // Applica la nuova posizione mantenendo l'asse Z originale del Transform
            spawnPoints[i].position = new Vector3(currentX, spawnY, spawnPoints[i].position.z);
        }
    }
}