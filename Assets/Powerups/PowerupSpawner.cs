using UnityEngine;
using UnityEngine.Pool;
using System; // Necessario per l'attributo [Serializable]

public class PowerupSpawner : MonoBehaviour
{
    public static PowerupSpawner Instance { get; private set; }

    // Struttura serializzabile per gestire prefabs e pesi dall'Inspector, come in PointSpawner
    [Serializable]
    public struct PowerupSpawnData
    {
        public GameObject prefab;
        [Tooltip("Peso per la probabilità di spawn. Valori più alti = spawn più frequente.")]
        public float spawnWeight; 
    }

    [Header("Prefabs Setup")]
    // Sostituiamo il vecchio array di GameObject con l'array della nuova struct
    [SerializeField] private PowerupSpawnData[] powerupPrefabsData; 

    private ObjectPool<GameObject>[] powerupPools;
    
    // Cache del peso totale per evitare calcoli ripetitivi a ogni spawn
    private float totalWeight = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Estraiamo i prefab dalla struct per passarli al MasterSpawner
        GameObject[] justPrefabs = new GameObject[powerupPrefabsData.Length];
        
        // Calcoliamo il peso totale e popoliamo l'array justPrefabs
        for (int i = 0; i < powerupPrefabsData.Length; i++)
        {
            totalWeight += powerupPrefabsData[i].spawnWeight;
            justPrefabs[i] = powerupPrefabsData[i].prefab; 
        }

        // Creiamo le pool utilizzando i prefab estratti
        powerupPools = MasterSpawner.Instance.CreatePoolsForPrefabs(justPrefabs, 5, 10);
    }

    // Metodo per ottenere una pool basata sul sistema di pesi
    public ObjectPool<GameObject> GetRandomPool()
    {
        // Controllo di sicurezza: se le pool non ci sono, ritorniamo null
        if (powerupPools == null || powerupPools.Length == 0) return null;

        // Estraiamo un valore casuale tra 0 e il peso totale
        // Usiamo UnityEngine.Random per evitare conflitti con System.Random
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // Iteriamo per trovare quale powerup corrisponde al valore estratto
        for (int i = 0; i < powerupPrefabsData.Length; i++)
        {
            currentWeight += powerupPrefabsData[i].spawnWeight;
            
            if (randomValue <= currentWeight)
            {
                return powerupPools[i];
            }
        }

        // Fallback di sicurezza in caso di rari errori di precisione dei float
        return powerupPools[0];
    }
}