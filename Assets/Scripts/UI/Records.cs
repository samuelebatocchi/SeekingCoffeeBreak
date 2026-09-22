using UnityEngine;

public static class Records
{
    public static int GetHighScore()
    {
        return (GameManager.Instance != null)
            ? GameManager.Instance.HighScore
            : PlayerPrefs.GetInt("HighScore", 0);
    }
}