using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Trascina qui l'oggetto ScoreText dalla Hierarchy")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Tooltip("Trascina qui l'oggetto PointsText dalla Hierarchy")]
    [SerializeField] private TextMeshProUGUI pointsText; // Riferimento per il testo dei punti

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScoreText(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore.ToString();
        }
        else
        {
            Debug.LogWarning("ScoreText non assegnato nell'UIManager!");
        }
    }

    // metodo per aggiornare la UI dei punti in tempo reale
    public void UpdatePointsText(int newTotalPoints)
    {
        if (pointsText != null)
        {
            // Aggiorna la stringa a schermo.
            pointsText.text = "Points: " + newTotalPoints.ToString();
        }
        else
        {
            Debug.LogWarning("PointsText non assegnato nell'UIManager!");
        }
    }
}