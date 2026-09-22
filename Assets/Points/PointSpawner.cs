using UnityEngine;
using UnityEngine.Pool;
using System; 

public class PointSpawner : MonoBehaviour
{
    public static PointSpawner Instance { get; private set; }

    // Struttura serializzabile per gestire prefabs e pesi direttamente dall'Inspector
    [Serializable]
    public struct PointSpawnData
    {
        public GameObject prefab;
        [Tooltip("Weight for spawn probability. Higher values = more frequent spawn.")]
        public float spawnWeight; 
    }

    [Header("Prefabs Setup")]
    [SerializeField] private PointSpawnData[] pointPrefabsData; 

    private ObjectPool<GameObject>[] pointPools;
    
    // Cache del peso totale per evitare ricalcoli in Update
    private float totalWeight = 0f;

    private void Awake()
    {
        // Pattern Singleton base
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Inizializza l'array temporaneo per passare i GameObject al MasterSpawner
        GameObject[] justPrefabs = new GameObject[pointPrefabsData.Length];
        
        // Calcola il totalWeight e popola l'array justPrefabs
        for (int i = 0; i < pointPrefabsData.Length; i++)
        {
            totalWeight += pointPrefabsData[i].spawnWeight;
            justPrefabs[i] = pointPrefabsData[i].prefab; 
        }

        // Usa MasterSpawner per creare i pool con l'array estratto
        pointPools = MasterSpawner.Instance.CreatePoolsForPrefabs(justPrefabs, 10, 30);
    }

    /// <summary>
    /// Restituisce un ObjectPool casuale basato sul sistema di pesi (Weighted Random Selection).
    /// </summary>
    public ObjectPool<GameObject> GetRandomPool()
    {
        // Early exit se i pool non sono inizializzati correttamente
        if (pointPools == null || pointPools.Length == 0) return null;

        // Specifica esplicitamente UnityEngine.Random per risolvere l'ambiguità con System.Random
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // Itera sull'array di setup per trovare il pool corrispondente al range estratto
        for (int i = 0; i < pointPrefabsData.Length; i++)
        {
            currentWeight += pointPrefabsData[i].spawnWeight;
            
            if (randomValue <= currentWeight)
            {
                return pointPools[i];
            }
        }

        // Fallback di sicurezza in caso di float precision error
        return pointPools[0];
    }
}