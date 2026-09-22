using UnityEngine;

// Questa è la classe base che viene spawnata nel mondo
public class PowerupItem_shield : PowerupItem
{
    public override void Activate(Character player)
    {
        // Controlliamo se il giocatore ha già uno scudo attivo per evitare duplicati
        if (player.GetComponent<ShieldEffect>() == null)
        {
            // Aggiungiamo DINAMICAMENTE il componente dello scudo al GameObject del giocatore
            player.gameObject.AddComponent<ShieldEffect>();
        }
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayShieldSound();
    }
}

// Questo è il comportamento dello scudo sul giocatore. 
// Esiste solo finché lo scudo è attivo e si autodistrugge quando viene colpito.
public class ShieldEffect : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Scudo equipaggiato sul personaggio!");
        // NOTA: Se in futuro vorrai aggiungere un effetto visivo (es. una bolla),
        // potrai istanziarla o gestirla direttamente qui dentro.
    }

    // Metodo per consumare e rimuovere lo scudo dal giocatore
    public void Consume()
    {
        Debug.Log("Scudo consumato e rimosso.");
        
        // Rimuove questo specifico componente dal giocatore
        Destroy(this);
    }
}