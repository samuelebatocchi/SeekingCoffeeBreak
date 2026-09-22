using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
 * Questa classe ha lo scopo di:
 * 1) posizionare il personaggio nella corretta posizione di inizio
 * 2) gestire la fisica del movimento tramite una curva di Bezier
 * 3) gestire il comportamento del personaggio al click (si sposta alla parete opposta)
 *
 * REFACTOR: tutto il calcolo delle posizioni avviene in WORLD SPACE,
 * eliminando le conversioni pixel <-> world che causavano disallineamenti.
 */
public class Character : MonoBehaviour
{
    Camera cam;
    private SpriteRenderer spriteRenderer;

    public float duration;
    private float elapsedTime = 0f;
    public float endX;      // world X di arrivo
    public float startX;    // world X di partenza

    [Header("Audio")]
    public AudioClip coffeeSound;
    public AudioClip tapSound;      // Suono riprodotto al cambio di direzione
    private AudioSource audioSource;
    public AudioMixerGroup sfxMixerGroup;

    // Percentuali viewport che devono coincidere esattamente con WallManager
    private const float VIEWPORT_LEFT  = 0.08f;
    private const float VIEWPORT_RIGHT = 0.92f;

    Transform s;
    bool isMoving = false;
    public InputAction clickAction;

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        s = gameObject.transform;

        // Inizializza o recupera l'AudioSource per riprodurre i suoni
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Inizializza il mixer
        if (sfxMixerGroup != null)
            audioSource.outputAudioMixerGroup = sfxMixerGroup;

        // Posiziona il personaggio a contatto con la parete destra, al 25% dell'altezza del Viewport
        float zDist = Mathf.Abs(cam.transform.position.z);
        Vector3 startPos = cam.ViewportToWorldPoint(new Vector3(VIEWPORT_RIGHT, 0.25f, zDist));
        startPos.x -= GetHalfCharacterWidthWorld(); // il bordo sinistro tocca la faccia interna del muro
        startPos.z = 0f;
        s.position = startPos;
    }

    // Ritorna la metà larghezza del personaggio in unità Unity (rispetta la Scale nell'Inspector)
    private float GetHalfCharacterWidthWorld()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer.bounds.size.x / 2f;
    }

    // Ritorna i due punti di arresto in WORLD SPACE:
    //   .x = stop contro il muro sinistro
    //   .y = stop contro il muro destro
    private Vector2 GetStopPositionsWorld()
    {
        float zDist = Mathf.Abs(cam.transform.position.z);

        // Facce interne dei muri in world space (coincidono con VIEWPORT_LEFT/RIGHT)
        float leftFaceX  = cam.ViewportToWorldPoint(new Vector3(VIEWPORT_LEFT,  0.5f, zDist)).x;
        float rightFaceX = cam.ViewportToWorldPoint(new Vector3(VIEWPORT_RIGHT, 0.5f, zDist)).x;

        float half = GetHalfCharacterWidthWorld();

        // Il centro del personaggio si ferma quando il suo bordo tocca la faccia del muro
        return new Vector2(leftFaceX + half, rightFaceX - half);
    }

    // Centro dello schermo in world X
    private float GetScreenCenterWorldX()
    {
        float zDist = Mathf.Abs(cam.transform.position.z);
        return cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, zDist)).x;
    }

    void Update()
    {
        // Abilita l'InputAction ad ogni frame per catturare l'input dell'utente
        clickAction.Enable();

        if (clickAction.WasPressedThisFrame())
        {
            // Riproduce il suono del tap se un AudioClip è stato assegnato via Inspector
            if (tapSound != null)
            {
                audioSource.PlayOneShot(tapSound);
            }

            // Imposta la nuova posizione di partenza nel punto esatto in cui avviene il click
            startX = s.position.x;
            elapsedTime = 0f;

            Vector2 stops = GetStopPositionsWorld();

            if (!isMoving)
            {
                // Se il personaggio è fermo, determina la direzione rispetto al centro dello schermo
                float centerX = GetScreenCenterWorldX();
                if (s.position.x >= centerX)
                    endX = stops.x;     // stop contro muro sinistro
                else
                    endX = stops.y;     // stop contro muro destro
            }
            else
            {
                // Se il personaggio è in volo, inverti la destinazione attuale
                if (Mathf.Approximately(endX, stops.x)) 
                    endX = stops.y;     // Stava andando a sinistra, ora va a destra
                else 
                    endX = stops.x;     // Stava andando a destra, ora va a sinistra
            }

            // Mantiene costante la velocità di spostamento scalando la duration
            float totalDistance = Mathf.Abs(stops.y - stops.x);
            float distanceToTravel = Mathf.Abs(endX - startX);
            
            // Applica la proporzione ai 0.3 secondi originali in base allo spazio effettivo da percorrere
            duration = 0.3f * (distanceToTravel / totalDistance);

            isMoving = true;
        }

        if (isMoving)
        {
            elapsedTime += Time.deltaTime;
            
            // Calcola il t normalizzato (0 a 1) per l'interpolazione
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedT = EaseInSine(t);

            // Lerp diretto in World Space verso il target
            float newX = Mathf.Lerp(startX, endX, easedT);
            s.position = new Vector3(newX, s.position.y, s.position.z);

            // Flip del Transform in base alla direzione dello spostamento
            float centerX = GetScreenCenterWorldX();
            if (newX >= centerX)
                s.eulerAngles = new Vector3(s.eulerAngles.x, 0f,   s.eulerAngles.z);
            else
                s.eulerAngles = new Vector3(s.eulerAngles.x, 180f, s.eulerAngles.z);

            // Arresto del movimento a destinazione raggiunta
            if (elapsedTime >= duration)
            {
                // Snap millimetrico alla posizione finale per evitare micro-gap visivi
                s.position = new Vector3(endX, s.position.y, s.position.z);
                isMoving = false;
            }
        }
    }

    // Funzione di Easing per addolcire la curva di movimento
    float EaseInSine(float t)
    {
        return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Gestione collisioni in base al tag del GameObject colpito
        if (collision.CompareTag("Obstacle"))
        {
            // Gestione Powerup Scudo
            if (TryGetComponent<ShieldEffect>(out ShieldEffect shield))
            {
                MasterSpawner.Instance.ReturnObjectToPool(collision.gameObject);
                shield.Consume();
                return; 
            }
            
            Debug.Log("Colpito ostacolo generato");
            GameManager.Instance.GameOver();
        }
        else if (collision.CompareTag("Point"))
        {
            Debug.Log("Punto ottenuto !");

            

            if (collision.TryGetComponent<PointItem>(out PointItem pointItem))
            {
                //se lo score è positivo riproduce suono dei punti
                if (pointItem.scoreValue >= 0 && coffeeSound != null)
                    audioSource.PlayOneShot(coffeeSound);

                GameManager.Instance.AddToWallet(pointItem.scoreValue);
            }
            else
            {
                //il point NON ha lo script PointItem (errore di setup)
                if (coffeeSound != null)
                    audioSource.PlayOneShot(coffeeSound);

                Debug.LogWarning("Il Prefab del Point non ha lo script PointItem attaccato!");
                GameManager.Instance.AddToWallet(1); 
            }
            
            // Restituisce l'oggetto al pooler
            MasterSpawner.Instance.ReturnObjectToPool(collision.gameObject);
        }
        else if (collision.CompareTag("Powerup"))
        {
            Debug.Log("Powerup ottenuto!");

            if (collision.TryGetComponent<PowerupItem>(out PowerupItem powerup))
            {
                powerup.Activate(this);
            }
            else
            {
                Debug.LogWarning("Il Prefab non ha uno script derivato da PowerupItem!");
            }

            // Restituisce l'oggetto al pooler
            MasterSpawner.Instance.ReturnObjectToPool(collision.gameObject);
        }
    }
}