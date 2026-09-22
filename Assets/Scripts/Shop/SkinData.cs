using UnityEngine;

public static class SkinData
{
    private const string EquippedKey = "EquippedSkin";
    private const string OwnedPrefix = "OwnedSkin_";

    public static string GetEquipped() => PlayerPrefs.GetString(EquippedKey, "suit");

    public static bool IsOwned(string skinId)
    {
        if (skinId == "suit") return true;
        return PlayerPrefs.GetInt(OwnedPrefix + skinId, 0) == 1;
    }

    public static void SetOwned(string skinId)
    {
        PlayerPrefs.SetInt(OwnedPrefix + skinId, 1);
        PlayerPrefs.Save();
    }

    public static void Equip(string skinId)
    {
        PlayerPrefs.SetString(EquippedKey, skinId);
        PlayerPrefs.Save();
    }
}