using UnityEngine;

/// <summary>
/// Muove e fa loopare il background esattamente come WallManager fa con i muri
/// (stessa velocità, stesso meccanismo A/B), ma posizionato al CENTRO dello
/// schermo (nello spazio di gioco tra i due muri) invece che ai bordi.
///
/// Non richiama SetWallSize: il dimensionamento verticale del background è già
/// gestito da TileVariationWall (che calcola la propria altezza con la stessa
/// formula usata qui, quindi i due combaciano senza bisogno di coordinarli).
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [Header("Background References")]
    public Transform backgroundA;
    public Transform backgroundB;

    private TileVariationWall tileGenA;
    private TileVariationWall tileGenB;
    private float bottomScreenY; // Bordo inferiore del Viewport in World Space

    void Start()
    {
        tileGenA = backgroundA.GetComponent<TileVariationWall>();
        tileGenB = backgroundB.GetComponent<TileVariationWall>();

        InitializeBackground();
    }

    void Update()
    {
        MoveBackground();
        LoopBackground();
    }

    void InitializeBackground()
    {
        Camera cam = Camera.main;
        float zDistance = Mathf.Abs(cam.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));

        bottomScreenY = bottomLeft.y;

        // A differenza dei muri (posizionati all'8%/92% del viewport), il
        // background va posizionato al CENTRO (50%), nello spazio di gioco
        // tra i due muri.
        float centerX = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, zDistance)).x;

        // Usiamo l'altezza REALE riportata da TileVariationWall (Scale inclusa),
        // non una nostra stima indipendente — così combacia sempre esattamente
        // con quello che si vede a schermo, qualunque Scale tu imposti.
        float heightA = GetSegmentHeight(tileGenA);
        float heightB = GetSegmentHeight(tileGenB);

        backgroundA.position = new Vector3(centerX, heightA / 2f, 0f);
        backgroundB.position = new Vector3(centerX, -heightB / 2f, 0f);
    }

    void MoveBackground()
    {
        // Stessa velocità globale usata da WallManager, quindi il background
        // scorre in perfetta sincronia con i muri (nessuno scivolamento relativo).
        float moveDistance = GameManager.Instance.CurrentSpeed * Time.deltaTime;
        Vector3 moveVector = Vector3.down * moveDistance;

        backgroundA.position += moveVector;
        backgroundB.position += moveVector;
    }

    void LoopBackground()
    {
        // Letta ad ogni frame (non cachata): se cambi la Scale in Inspector
        // (anche in pausa), il loop si adatta subito senza overlap né gap.
        float heightA = GetSegmentHeight(tileGenA);
        float heightB = GetSegmentHeight(tileGenB);

        if (backgroundA.position.y + heightA / 2f < bottomScreenY)
            backgroundA.position = new Vector3(backgroundA.position.x, backgroundB.position.y + heightB, 0f);

        if (backgroundB.position.y + heightB / 2f < bottomScreenY)
            backgroundB.position = new Vector3(backgroundB.position.x, backgroundA.position.y + heightA, 0f);
    }

    float GetSegmentHeight(TileVariationWall gen)
    {
        // Fallback di sicurezza nel caso manchi il componente su uno dei due oggetti.
        return gen != null ? gen.WorldHeight : 0f;
    }
}