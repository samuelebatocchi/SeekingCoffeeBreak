using UnityEngine;

public class ScreenSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject shopPanel;

    public void ShowShop()
    {
        homePanel.SetActive(true);
        shopPanel.SetActive(true);
    }

    public void ShowHome()
    {
        shopPanel.SetActive(false);
        homePanel.SetActive(true);
    }
}