using UnityEngine;

// Classe base astratta per il polimorfismo dei powerup
//I prefab avranno uno script che eredita da questa classe
public abstract class PowerupItem : MonoBehaviour
{
    // Metodo chiamato dal Character al momento della collisione
    public abstract void Activate(Character player);
}