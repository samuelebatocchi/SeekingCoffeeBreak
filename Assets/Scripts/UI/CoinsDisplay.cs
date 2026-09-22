using UnityEngine;
using TMPro;

public class CoinsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinValueText;
    [SerializeField] private string labelPrefix = "";

    private void OnEnable()
    {
        Refresh();
        Wallet.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        Wallet.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        coinValueText.text = labelPrefix + Wallet.GetBalance();
    }
}