using UnityEngine;
using System;

public static class Wallet
{
    public static event Action OnChanged;

    public static int GetBalance()
    {
        return (GameManager.Instance != null)
            ? GameManager.Instance.TotalPoints
            : PlayerPrefs.GetInt("TotalPoints", 0);
    }

    public static bool HasEnough(int amount) => GetBalance() >= amount;

    public static void Spend(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddToWallet(-amount);
        }
        else
        {
            int newBalance = GetBalance() - amount;
            PlayerPrefs.SetInt("TotalPoints", newBalance);
            PlayerPrefs.Save();
        }
        OnChanged?.Invoke();
    }
}