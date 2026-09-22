using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudDataSync : MonoBehaviour
{
    public static CloudDataSync Instance { get; private set; }

    // Elenca qui tutti gli id delle skin esistenti nel gioco (esclusa "suit", sempre posseduta di default)
    [SerializeField] private string[] _allSkinIds;

    private const string CoinsKey = "coins";
    private const string HighScoreKey = "highScore";
    private const string EquippedSkinKey = "equippedSkin";
    private const string OwnedSkinsKey = "ownedSkins";

    private bool _syncEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Wallet.OnChanged += HandleWalletChanged;
    }

    private void OnDisable()
    {
        Wallet.OnChanged -= HandleWalletChanged;
    }

    // --- LOGIN ---

    public async Task SyncOnLoginAsync()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var cloud = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { CoinsKey, HighScoreKey, EquippedSkinKey, OwnedSkinsKey });

        // Coins: vince il valore piu' alto tra locale e cloud
        int localCoins = Wallet.GetBalance();
        int resolvedCoins = cloud.TryGetValue(CoinsKey, out var coinsItem)
            ? Mathf.Max(coinsItem.Value.GetAs<int>(), localCoins)
            : localCoins;
        WriteCoins(resolvedCoins);

        // High score: vince il valore piu' alto tra locale e cloud
        int localHighScore = Records.GetHighScore();
        int resolvedHighScore = cloud.TryGetValue(HighScoreKey, out var hsItem)
            ? Mathf.Max(hsItem.Value.GetAs<int>(), localHighScore)
            : localHighScore;
        WriteHighScore(resolvedHighScore);

        // Skin possedute: unione tra locali e cloud, nessuna va persa
        var ownedFromCloud = cloud.TryGetValue(OwnedSkinsKey, out var ownedItem)
            ? ownedItem.Value.GetAs<List<string>>()
            : new List<string>();

        foreach (var skinId in ownedFromCloud)
        {
            SkinData.SetOwned(skinId);
        }

        // Skin equipaggiata: se il locale e' ancora il default, prendi quella dal cloud
        if (cloud.TryGetValue(EquippedSkinKey, out var eqItem))
        {
            string cloudEquipped = eqItem.Value.GetAs<string>();
            if (SkinData.GetEquipped() == "suit" && cloudEquipped != "suit")
            {
                SkinData.Equip(cloudEquipped);
            }
        }

        _syncEnabled = true;

        // Riscrive sul cloud i valori risolti (utile al primo login o dopo un merge)
        await SaveAllAsync();
    }

    // --- LOGOUT ---

    public void Reset()
    {
        _syncEnabled = false;
    }

    // --- SAVE ---

    // Chiamalo dopo ogni acquisto/equip skin e dopo ogni aggiornamento high score,
    // dato che SkinData e Records non espongono eventi
    public async void RequestSave()
    {
        if (!_syncEnabled) return;
        await SaveAllAsync();
    }

    private async void HandleWalletChanged()
    {
        await SaveAllAsync();
    }

    private async Task SaveAllAsync()
    {
        if (!_syncEnabled) return;
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var ownedSkins = _allSkinIds.Where(SkinData.IsOwned).ToList();

        var data = new Dictionary<string, object>
        {
            { CoinsKey, Wallet.GetBalance() },
            { HighScoreKey, Records.GetHighScore() },
            { EquippedSkinKey, SkinData.GetEquipped() },
            { OwnedSkinsKey, ownedSkins }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log($"[CloudDataSync] Salvato su cloud: coins={data[CoinsKey]}, highScore={data[HighScoreKey]}, skin={data[EquippedSkinKey]}, owned=[{string.Join(",", ownedSkins)}]");
    }

    // --- Scrittura diretta: replica la logica interna di storage di Wallet/Records,
    // dato che quelle classi non espongono un setter pubblico generico ---

    private void WriteCoins(int newBalance)
    {
        if (GameManager.Instance != null)
        {
            int delta = newBalance - GameManager.Instance.TotalPoints;
            GameManager.Instance.AddToWallet(delta);
        }
        else
        {
            PlayerPrefs.SetInt("TotalPoints", newBalance);
            PlayerPrefs.Save();
        }
    }

    private void WriteHighScore(int newScore)
    {
        if (GameManager.Instance != null)
        {
            // GameManager.HighScore ha il setter non pubblico: non possiamo scriverci
            // direttamente da qui. Se il tuo gioco usa davvero un'istanza di GameManager
            // attiva, aggiungi un metodo pubblico tipo "SetHighScore(int value)" a
            // GameManager e sostituisci la riga sotto con quella chiamata.
            Debug.LogWarning(
                "CloudDataSync: impossibile scrivere GameManager.HighScore (setter non pubblico). " +
                "Il valore risolto dal cloud non è stato applicato a GameManager.");
        }
        else
        {
            PlayerPrefs.SetInt("HighScore", newScore);
            PlayerPrefs.Save();
        }
    }
}