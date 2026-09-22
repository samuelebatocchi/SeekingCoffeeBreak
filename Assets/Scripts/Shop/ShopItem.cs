using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("Skin Info")]
    [SerializeField] private string skinId;
    [SerializeField] private int price;

    [Header("References")]
    [SerializeField] private Button priceTagButton;
    [SerializeField] private TextMeshProUGUI priceText;

    private static ShopItem[] allItems;

    private void OnEnable()
    {
        RefreshAllStatic();
    }

    private void RefreshAllStatic()
    {
        allItems = FindObjectsByType<ShopItem>(FindObjectsSortMode.None);
        foreach (var item in allItems) item.RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (!SkinData.IsOwned(skinId))
        {
            priceText.text = price.ToString();
        }
        else if (SkinData.GetEquipped() == skinId)
        {
            priceText.text = "Equipped";
        }
        else
        {
            priceText.text = "Equip";
        }
    }

    public void OnClickPriceTag()
    {
        if (SkinData.GetEquipped() == skinId) return;

        if (SkinData.IsOwned(skinId))
        {
            SkinData.Equip(skinId);
            RefreshAllStatic();
            return;
        }

        // Se non è posseduta: prova ad acquistarla
        if (Wallet.HasEnough(price))
        {
            Wallet.Spend(price);
            SkinData.SetOwned(skinId);
            SkinData.Equip(skinId);
            RefreshAllStatic();
        }
        else
        {
            FlashRed();
        }
    }

    private void FlashRed()
    {
        priceText.color = Color.red;
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.3f);
    }

    private void ResetColor()
    {
        priceText.color = Color.black;
    }
}