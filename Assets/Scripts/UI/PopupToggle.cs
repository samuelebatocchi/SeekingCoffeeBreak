using UnityEngine;

public class PopupToggle : MonoBehaviour
{
    [SerializeField] private GameObject popup;

    public void Open()
    {
        popup.SetActive(true);
    }

    public void Close()
    {
        popup.SetActive(false);
    }
}