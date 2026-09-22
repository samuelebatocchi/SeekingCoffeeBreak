using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Genera automaticamente una colonna di tile (sprite diversi, scelti da un array)
/// che riempiono esattamente l'altezza del segmento muro, senza dipendere né
/// modificare WallManager.
///
/// Va messo su "wall" e "wall (1)" al posto dei 12 figli piazzati a mano.
/// Il parent (wall / wall(1)) può tenere il suo SpriteRenderer com'è
/// (anche con Sprite = None: non serve che sia visibile, WallManager può
/// continuare a scrivere sr.size su di esso senza alcun effetto collaterale).
///
/// Ricalcola l'altezza schermo con la STESSA formula di WallManager, quindi
/// il numero di tile generate corrisponde sempre esattamente a wallHeight,
/// eliminando il gap che si vedeva nel video.
/// </summary>
public class TileVariationWall : MonoBehaviour
{
    [System.Serializable]
    public class TileOption
    {
        public Sprite sprite;
        [Tooltip("Peso relativo: più alto = più probabile. 1 = normale, 0.2 = raro")]
        public float weight = 1f;
    }

    public enum SelectionMode
    {
        Random,   // pesca casualmente in base ai weight
        Sequence  // segue un ordine fisso e ripetuto, sempre uguale
    }

    [Header("Modalità di selezione")]
    public SelectionMode selectionMode = SelectionMode.Sequence;

    [Header("Tile disponibili (trascina qui gli sprite, es. ceiling_office_C, ceiling_office_B)")]
    [Tooltip("In modalità Sequence, l'ORDINE in cui li metti qui è l'ordine in cui verranno ripetuti dal basso verso l'alto (weight ignorato).")]
    public List<TileOption> tileOptions = new List<TileOption>();

    [Header("Dimensioni")]
    [Tooltip("Spessore del muro (larghezza in world units a cui viene adattata ogni tile). Ignorato se 'Preserve Aspect Ratio' è attivo.")]
    public float wallThickness = 1f;

    [Tooltip("Se attivo, ignora Wall Thickness e scala la tile con lo STESSO fattore su X e Y, mantenendo le proporzioni naturali dello sprite (utile per background/decorazioni con tile quadrate o non sottili come i muri).")]
    public bool preserveAspectRatio = false;

    [Header("Sorting")]
    public string sortingLayerName = "Default";
    public int orderInLayer = 1;

    [Header("Random")]
    public int randomSeed = 0; // 0 = casuale ogni volta, altrimenti deterministico (utile per debug)

    [Header("Pattern")]
    [Tooltip("Se attivo, garantisce che il numero di tile generate per segmento sia sempre pari, aggiungendone una in più (continuando la sequenza) quando il conteggio naturale risulterebbe dispari.")]
    public bool forceEvenTileCount = false;

    private float computedWallHeight;
    private float targetLocalHeight;
    private float targetLocalThickness;

    /// <summary>
    /// Altezza REALE occupata da questo oggetto in world space, Scale del Transform
    /// inclusa. Va usata da script esterni (es. BackgroundManager) per posizionare
    /// correttamente i segmenti in loop, invece di ricalcolare l'altezza schermo
    /// per conto proprio — altrimenti la Scale di questo oggetto e la spaziatura
    /// calcolata altrove finiscono per non coincidere, causando overlap o gap.
    /// Valutata "live" (non cachata) così riflette anche cambi di Scale fatti a
    /// runtime, es. in pausa dall'Inspector.
    /// </summary>
    public float WorldHeight => computedWallHeight * transform.lossyScale.y;

    void Awake()
    {
        // Awake gira sempre PRIMA di qualsiasi Start() in scena (garanzia di Unity),
        // quindi quando BackgroundManager.Start() legge WorldHeight, il valore è
        // già pronto — evita la race condition che causava overlap all'avvio.
        if (randomSeed != 0)
            Random.InitState(randomSeed + GetInstanceID());

        computedWallHeight = ComputeScreenWallHeight();

        if (tileOptions.Count == 0)
        {
            Debug.LogWarning($"{name}: nessuna tile in Tile Options, impossibile generare il muro.");
            return;
        }

        // NOTA: qui NON compensiamo la Scale del genitore. Il contenuto viene
        // generato per riempire esattamente computedWallHeight in spazio locale,
        // e la Scale del Transform (impostata da te in Inspector) lo ingrandisce
        // o rimpicciolisce normalmente come farebbe con qualsiasi altro oggetto
        // Unity — così il risultato è coerente sia che tu imposti la Scale prima
        // di Play, sia che la cambi mentre il gioco è in esecuzione.
        targetLocalHeight = computedWallHeight;
        targetLocalThickness = wallThickness;

        GenerateTiles();
    }

    // Stessa identica formula usata in WallManager.InitializeWalls(), duplicata
    // qui apposta per restare completamente indipendenti dallo script esistente.
    float ComputeScreenWallHeight()
    {
        Camera cam = Camera.main;
        float zDistance = Mathf.Abs(cam.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(0, 1, zDistance));

        return Vector3.Distance(bottomLeft, topLeft) + 2f;
    }

    void GenerateTiles()
    {
        // Rimuove eventuali figli preesistenti (utile se richiami GenerateTiles più volte,
        // es. da editor per anteprima)
        for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(childIndex).gameObject);
#else
            Destroy(transform.GetChild(childIndex).gameObject);
#endif
        }

        int safetyLimit = 2000; // evita loop infiniti se qualcosa va storto (es. sprite con bounds 0)

        // --- PASS 1: decidiamo QUALI tile servono e la loro altezza naturale totale,
        // senza ancora istanziare nulla. Questo ci serve per poter poi normalizzare
        // l'altezza totale esattamente a computedWallHeight (vedi PASS 2).
        List<Sprite> chosenSprites = new List<Sprite>();
        float naturalHeight = 0f;
        int idx = 0;

        while (naturalHeight < targetLocalHeight && idx < safetyLimit)
        {
            Sprite s = PickTile(idx);
            if (s == null) break;

            float h = s.bounds.size.y;
            if (h <= 0f) break; // sprite non valido, evita loop infinito

            chosenSprites.Add(s);
            naturalHeight += h;
            idx++;
        }

        if (chosenSprites.Count == 0 || naturalHeight <= 0f) return;

        // Se richiesto, forziamo un numero PARI di tile aggiungendone una in più
        // (proseguendo semplicemente la sequenza/random da dove eravamo rimasti),
        // così la normalizzazione sotto continua a far combaciare tutto esattamente
        // con targetLocalHeight, ma con un conteggio pari.
        if (forceEvenTileCount && chosenSprites.Count % 2 != 0 && idx < safetyLimit)
        {
            Sprite extra = PickTile(idx);
            if (extra != null && extra.bounds.size.y > 0f)
            {
                chosenSprites.Add(extra);
                naturalHeight += extra.bounds.size.y;
                idx++;
            }
        }

        // --- PASS 2: scaliamo TUTTE le tile con lo stesso fattore, così la somma
        // delle loro altezze combacia esattamente con targetLocalHeight (l'altezza
        // world-space desiderata, già compensata per la Scale del genitore).
        // Questo elimina sia i gap che gli overlap tra un segmento e il successivo.
        float scaleFactor = targetLocalHeight / naturalHeight;

        float currentY = -targetLocalHeight / 2f;

        for (int i = 0; i < chosenSprites.Count; i++)
        {
            Sprite chosen = chosenSprites[i];
            float scaledHeight = chosen.bounds.size.y * scaleFactor;

            GameObject tileGO = new GameObject($"Tile_{i}");
            tileGO.transform.SetParent(transform, false);
            tileGO.transform.localPosition = new Vector3(0f, currentY + scaledHeight / 2f, 0f);

            SpriteRenderer sr = tileGO.AddComponent<SpriteRenderer>();
            sr.sprite = chosen;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = orderInLayer;

            // Larghezza: o adattata a targetLocalThickness (comportamento originale,
            // per muri sottili — anch'esso compensato per la Scale del genitore),
            // oppure scalata con lo STESSO fattore dell'altezza per mantenere le
            // proporzioni naturali dello sprite (tile quadrate/non distorte).
            float spriteWidth = chosen.bounds.size.x;
            float scaleX = preserveAspectRatio
                ? scaleFactor
                : (spriteWidth > 0f ? targetLocalThickness / spriteWidth : 1f);
            tileGO.transform.localScale = new Vector3(scaleX, scaleFactor, 1f);

            currentY += scaledHeight;
        }
    }

    Sprite PickTile(int index)
    {
        if (tileOptions.Count == 0) return null;

        if (selectionMode == SelectionMode.Sequence)
        {
            // Ciclo fisso e ripetuto sull'ordine degli elementi in tileOptions.
            // Es. con [C, C, B] genera C, C, B, C, C, B, C, C, B, ... sempre identico,
            // a prescindere da wallHeight o da quante volte fai Play.
            var opt = tileOptions[index % tileOptions.Count];
            return opt.sprite;
        }

        return PickRandomTile();
    }

    Sprite PickRandomTile()
    {
        if (tileOptions.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var opt in tileOptions)
            if (opt.sprite != null) totalWeight += Mathf.Max(0f, opt.weight);

        if (totalWeight <= 0f) return tileOptions[0].sprite;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        foreach (var opt in tileOptions)
        {
            if (opt.sprite == null) continue;
            cumulative += Mathf.Max(0f, opt.weight);
            if (roll <= cumulative) return opt.sprite;
        }

        return tileOptions[tileOptions.Count - 1].sprite;
    }
}