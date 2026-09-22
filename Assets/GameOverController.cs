using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI newRecordText; // testo "NEW RECORD!!" separato, accanto allo score

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Show(int score, bool isNewRecord)
    {
        gameOverScreen.SetActive(true);
        scoreText.text = "SCORE: " + score;
        newRecordText.gameObject.SetActive(isNewRecord);
        Time.timeScale = 0f; // congela il gameplay dietro la schermata
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu"); // da confermare il nome esatto
    }
}