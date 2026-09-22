using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [Header("Global Speed Settings")]
    [SerializeField] private float baseSpeed = 5f;
    public float speedMultiplier;
    [SerializeField] private float speedIncreaseRate = 0.1f;

    [Header("Score Settings")]
    public int Score { get; private set; }

    public int HighScore { get; private set; }
    public int TotalPoints { get; private set; }

    [Header("Negative Score Feedback")]
    [SerializeField] private ScreenShaker screenShaker;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 15f;

    public float CurrentSpeed => baseSpeed + speedMultiplier;
    private float scoreTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f;

        HighScore = PlayerPrefs.GetInt("HighScore", 0);
        TotalPoints = PlayerPrefs.GetInt("TotalPoints", 0);
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreText(Score);
            UIManager.Instance.UpdatePointsText(TotalPoints);
        }
    }

    private void Update()
    {
        speedMultiplier += Time.deltaTime * speedIncreaseRate;

        scoreTimer += Time.unscaledDeltaTime;
        if (scoreTimer >= 1f)
        {
            AddScore(1);
            scoreTimer -= 1f;
        }
    }

    public void AddScore(int amount)
    {
        Score += amount;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreText(Score);
        }
    }

    public void AddToWallet(int amount)
    {
        TotalPoints += amount;

        PlayerPrefs.SetInt("TotalPoints", TotalPoints);
        PlayerPrefs.Save();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePointsText(TotalPoints);
        }

        if (amount < 0)
        {
            MusicManager.Instance.PlayNegativeScoreSound();
            if (screenShaker != null)
            {
                StartCoroutine(screenShaker.Shake(shakeDuration, shakeMagnitude));  //per implementazione futura
            }
        }
    }

    public void GameOver()
    {
        bool isNewRecord = Score > HighScore;
        if (isNewRecord)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }

        GameOverController.Instance.Show(Score, isNewRecord);
        MusicManager.Instance.StopMusic();
        MusicManager.Instance.PlayGameOverMusic();
    }

    public void ResetScore()
    {
        Score = 0;
        scoreTimer = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreText(Score);
        }
    }
}