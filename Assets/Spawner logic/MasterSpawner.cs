using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MasterSpawner : MonoBehaviour
{
    public static MasterSpawner Instance { get; private set; }

    [Header("Global Settings")]
    [SerializeField] private Transform[] spawnPoints;
    
    // Aggiungiamo un parametro per regolare l'offset di posizionamento al volo, 
    // utile per allontanare gli ostacoli ruotati dai collider dei margini.
    [Header("Spawn Position Adjustments")]
    [Tooltip("Offset sull'asse X per evitare la collisione con i margini quando l'oggetto viene ruotato.")]
    [SerializeField] private float sideSpawnOffset = 0.5f;

    [Header("Spawn Timers")]
    [SerializeField] private float timeBetweenObstacles = 2f;
    [SerializeField] private float timeBetweenPoints = 4f;
    [SerializeField] private float timeBetweenPowerups = 12f;

    [Header("Spawn Probabilities")]
    [Tooltip("Probabilità di spawn per ogni oggetto allo scadere del timer. 0.8 = 80%.")]
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.8f;

    [Header("Fast Obstacles Settings")]
    [Tooltip("Probabilità che un ostacolo spawnato sia veloce (0.2 = 20%).")]
    [SerializeField, Range(0f, 1f)] private float fastObstacleChance = 0.2f;
    
    [Tooltip("Moltiplicatore minimo di velocità per gli ostacoli veloci.")]
    [SerializeField] private float minFastMultiplier = 1.2f; 
    [Tooltip("Moltiplicatore massimo di velocità per gli ostacoli veloci.")]
    [SerializeField] private float maxFastMultiplier = 5.0f;

    [Header("Exclusion Rules")]
    [Tooltip("Cooldown globale: secondi per cui uno spawner è considerato occupato dopo aver generato QUALSIASI oggetto.")]
    [SerializeField] private float safeCooldown = 1.5f;

    [Header("Point Frenzy Settings")]
    [Tooltip("Numero minimo di punti spawnati per tick durante il Frenzy.")]
    [SerializeField] private int minFrenzyPointsPerSpawn = 1;
    [Tooltip("Numero massimo di punti spawnati per tick durante il Frenzy.")]
    [SerializeField] private int maxFrenzyPointsPerSpawn = 5;

    private float obstacleTimer;
    private float pointTimer;
    private float powerupTimer;

    private float[] lastSpawnTimes;
    private Dictionary<GameObject, ObjectPool<GameObject>> activeObjectsTracker = new Dictionary<GameObject, ObjectPool<GameObject>>();
    private List<int> availableIndices;

    private bool isPointFrenzyActive = false;
    private float originalTimeBetweenPoints;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (spawnPoints != null)
        {
            lastSpawnTimes = new float[spawnPoints.Length];
            availableIndices = new List<int>(spawnPoints.Length);

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                lastSpawnTimes[i] = -100f;
            }
        }

        originalTimeBetweenPoints = timeBetweenPoints;
    }

    private void Update()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        HandleObstaclesSpawn();
        HandlePointsSpawn();
        HandlePowerupsSpawn();
    }

    private void HandleObstaclesSpawn()
    {
        if (isPointFrenzyActive) return;

        obstacleTimer += Time.deltaTime;
        if (obstacleTimer >= timeBetweenObstacles)
        {
            obstacleTimer = 0f;

            if (Random.value <= spawnChance && ObstacleSpawner.Instance != null)
            {
                PopulateAvailableIndices();

                // Calcolo quantità da spawnare e check di sicurezza sui pool disponibili
                int amountToSpawn = (Random.value <= 0.5f) ? 1 : 2;
                amountToSpawn = Mathf.Min(amountToSpawn, availableIndices.Count);
                if (amountToSpawn == 0) return;

                List<int> selectedIndices = new List<int>();
                int lastIndex = spawnPoints.Length - 1;

                // Estrazione dinamica degli indici con controllo della restrizione integrato
                while (selectedIndices.Count < amountToSpawn && availableIndices.Count > 0)
                {
                    // Scegliamo e rimuoviamo un indice casuale dai disponibili
                    int randomListIndex = Random.Range(0, availableIndices.Count);
                    int chosenSpawnIndex = availableIndices[randomListIndex];
                    availableIndices.RemoveAt(randomListIndex);

                    // Regola specifica: se abbiamo già l'estremo sinistro (0), rifiutiamo l'estremo destro (lastIndex) e viceversa.
                    if (chosenSpawnIndex == lastIndex && selectedIndices.Contains(0)) continue;
                    if (chosenSpawnIndex == 0 && selectedIndices.Contains(lastIndex)) continue;

                    // Se l'indice supera i controlli, è valido per lo spawn
                    selectedIndices.Add(chosenSpawnIndex);
                }

                // Esecuzione dello spawn per tutti gli indici validati
                foreach (int spawnIndex in selectedIndices)
                {
                    ObjectPool<GameObject> pool = ObstacleSpawner.Instance.GetRandomPool();
                    if (pool == null) continue;

                    GameObject spawnedObj = SpawnAtPoint(pool, spawnIndex);

                    if (spawnedObj.TryGetComponent<ObstacleMovement>(out ObstacleMovement obstacleMovement))
                    {
                        if (Random.value <= fastObstacleChance)
                        {
                            obstacleMovement.localSpeedMultiplier = Random.Range(minFastMultiplier, maxFastMultiplier);
                        }
                    }
                    
                    // Aggiorniamo il cooldown dello spawner utilizzato
                    lastSpawnTimes[spawnIndex] = Time.time;
                }
            }
        }
    }

    private void HandlePointsSpawn()
    {
        pointTimer += Time.deltaTime;
        if (pointTimer >= timeBetweenPoints)
        {
            pointTimer = 0f;

            if ((isPointFrenzyActive || Random.value <= spawnChance) && PointSpawner.Instance != null)
            {
                PopulateAvailableIndices();

                int amountToSpawn = isPointFrenzyActive 
                    ? Random.Range(minFrenzyPointsPerSpawn, maxFrenzyPointsPerSpawn + 1)
                    : 1;

                for (int i = 0; i < amountToSpawn; i++)
                {
                    if (availableIndices.Count == 0) break; 

                    ObjectPool<GameObject> pool = PointSpawner.Instance.GetRandomPool();
                    if (pool == null) continue;

                    int listIndex = Random.Range(0, availableIndices.Count);
                    int spawnIndex = availableIndices[listIndex];
                    
                    availableIndices.RemoveAt(listIndex);

                    SpawnAtPoint(pool, spawnIndex);
                    lastSpawnTimes[spawnIndex] = Time.time;
                }
            }
        }
    }

    private void HandlePowerupsSpawn()
    {
        if (isPointFrenzyActive) return;

        powerupTimer += Time.deltaTime;
        if (powerupTimer >= timeBetweenPowerups)
        {
            powerupTimer = 0f;

            if (Random.value <= spawnChance && PowerupSpawner.Instance != null)
            {
                ObjectPool<GameObject> pool = PowerupSpawner.Instance.GetRandomPool();
                if (pool == null) return;

                PopulateAvailableIndices();

                if (availableIndices.Count > 0)
                {
                    int spawnIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                    SpawnAtPoint(pool, spawnIndex);
                    
                    lastSpawnTimes[spawnIndex] = Time.time;
                }
            }
        }
    }

    private void PopulateAvailableIndices()
    {
        availableIndices.Clear();
        float currentCooldown = isPointFrenzyActive ? 0.1f : safeCooldown;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (Time.time - lastSpawnTimes[i] >= currentCooldown)
            {
                availableIndices.Add(i);
            }
        }
    }

    private GameObject SpawnAtPoint(ObjectPool<GameObject> pool, int pointIndex)
    {
        GameObject spawnedObj = pool.Get();
        
        activeObjectsTracker.TryAdd(spawnedObj, pool);

        // Catturiamo la posizione originale dello spawner
        Vector3 finalPosition = spawnPoints[pointIndex].position;

        // Gestione della rotazione e aggiustamento della posizione (Offset) per evitare collisioni immediate
        if (pointIndex == 0)
        {
            spawnedObj.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            // Sposta l'oggetto verso destra allontanandolo dal margine sinistro
            finalPosition.x += sideSpawnOffset; 
        }
        else if (pointIndex == spawnPoints.Length - 1)
        {
            spawnedObj.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            // Sposta l'oggetto verso sinistra allontanandolo dal margine destro
            finalPosition.x -= sideSpawnOffset;
        }
        else
        {
            spawnedObj.transform.rotation = Quaternion.identity;
        }

        // Assegniamo la posizione finale ricalcolata
        spawnedObj.transform.position = finalPosition;

        return spawnedObj;
    }

    public ObjectPool<GameObject>[] CreatePoolsForPrefabs(GameObject[] prefabs, int defaultCap = 10, int maxCap = 30)
    {
        ObjectPool<GameObject>[] pools = new ObjectPool<GameObject>[prefabs.Length];

        for (int i = 0; i < prefabs.Length; i++)
        {
            int index = i; 
            pools[i] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefabs[index]),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: defaultCap,
                maxSize: maxCap
            );
        }
        return pools;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        if (activeObjectsTracker.TryGetValue(obj, out ObjectPool<GameObject> pool))
        {
            pool.Release(obj);
            activeObjectsTracker.Remove(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    public void ActivatePointFrenzy(float duration, float spawnInterval)
    {
        StopCoroutine(nameof(PointFrenzyRoutine)); 
        StartCoroutine(PointFrenzyRoutine(duration, spawnInterval));
    }

    private System.Collections.IEnumerator PointFrenzyRoutine(float duration, float spawnInterval)
    {
        isPointFrenzyActive = true;
        
        pointTimer = timeBetweenPoints; 
        timeBetweenPoints = spawnInterval; 

        yield return new WaitForSeconds(duration);

        isPointFrenzyActive = false;
        timeBetweenPoints = originalTimeBetweenPoints;
    }
}