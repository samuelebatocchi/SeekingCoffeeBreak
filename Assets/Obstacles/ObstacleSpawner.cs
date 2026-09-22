using UnityEngine;
using UnityEngine.Pool;

public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance { get; private set; }

    [Header("Prefabs Setup")]
    [SerializeField] private GameObject[] obstaclePrefabs; 

    private ObjectPool<GameObject>[] obstaclePools;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Inizializza i pool delegando la creazione al MasterSpawner
        obstaclePools = MasterSpawner.Instance.CreatePoolsForPrefabs(obstaclePrefabs, 10, 30);
    }

    /// <summary>
    /// Restituisce un pool casuale tra quelli configurati.
    /// Chiamato dal MasterSpawner quando è il momento di spawnare.
    /// </summary>
    public ObjectPool<GameObject> GetRandomPool()
    {
        if (obstaclePools == null || obstaclePools.Length == 0) return null;
        int randomIndex = Random.Range(0, obstaclePools.Length);
        return obstaclePools[randomIndex];
    }
}