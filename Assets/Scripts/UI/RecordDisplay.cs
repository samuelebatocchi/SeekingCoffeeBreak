using UnityEngine;
using TMPro;

public class RecordDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recordValueText;

    private void OnEnable()
    {
        recordValueText.text = Records.GetHighScore().ToString();
    }
}