using UnityEngine;

public class WallManager : MonoBehaviour
{
    [Header("Wall References")]
    public Transform leftWallA;
    public Transform leftWallB;
    public Transform rightWallA;
    public Transform rightWallB;

    [Header("Wall Settings")]
    [Tooltip("Spessore dei muri in unità di Unity")]
    public float wallThickness = 1f;

    private float wallHeight;
    private float bottomScreenY;

    void Start()
    {
        InitializeWalls();
    }

    void Update()
    {
        MoveWalls();
        LoopWalls();
    }

    void InitializeWalls()
    {
        Camera cam = Camera.main;
        float zDistance = Mathf.Abs(cam.transform.position.z);

        // 1. Calcolo limiti dello schermo in World Space
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, zDistance));
        
        // Setup altezze
        wallHeight = Vector3.Distance(bottomLeft, topLeft) + 2f;
        bottomScreenY = bottomLeft.y;

        // 2. Calcolo posizioni X (8% e 92% del Viewport) compensando lo spessore
        float leftX = cam.ViewportToWorldPoint(new Vector3(0.08f, 0, zDistance)).x - (wallThickness / 2f);
        float rightX = cam.ViewportToWorldPoint(new Vector3(0.92f, 0, zDistance)).x + (wallThickness / 2f);

        // 3. Applicazione dimensioni ai SpriteRenderer
        SetWallSize(leftWallA);
        SetWallSize(leftWallB);
        SetWallSize(rightWallA);
        SetWallSize(rightWallB);

        // 4. Posizionamento iniziale definitivo
        // Il segmento A parte verso l'alto, il segmento B verso il basso
        leftWallA.position = new Vector3(leftX, wallHeight / 2f, 0f);
        leftWallB.position = new Vector3(leftX, -wallHeight / 2f, 0f);
        
        rightWallA.position = new Vector3(rightX, wallHeight / 2f, 0f);
        rightWallB.position = new Vector3(rightX, -wallHeight / 2f, 0f);
    }

    void SetWallSize(Transform wall)
    {
        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.size = new Vector2(wallThickness, wallHeight);
        }
    }

    void MoveWalls()
    {
        // Legge la velocità globale dal GameManager
        float moveDistance = GameManager.Instance.CurrentSpeed * Time.deltaTime;
        Vector3 moveVector = Vector3.down * moveDistance;

        leftWallA.position += moveVector;
        leftWallB.position += moveVector;
        rightWallA.position += moveVector;
        rightWallB.position += moveVector;
    }

    void LoopWalls()
    {
        LoopPair(leftWallA, leftWallB);
        LoopPair(rightWallA, rightWallB);
    }

    void LoopPair(Transform a, Transform b)
    {
        // Quando il bordo superiore del segmento scende sotto il fondo schermo,
        // lo riposizioniamo esattamente sopra l'altro segmento.
        float halfHeight = wallHeight / 2f;

        if (a.position.y + halfHeight < bottomScreenY)
            a.position = new Vector3(a.position.x, b.position.y + wallHeight, 0f);

        if (b.position.y + halfHeight < bottomScreenY)
            b.position = new Vector3(b.position.x, a.position.y + wallHeight, 0f);
    }
}