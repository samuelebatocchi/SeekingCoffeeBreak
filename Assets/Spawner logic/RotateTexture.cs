using UnityEngine;

public class RotateTexture : MonoBehaviour
{
    public string texturePropertyName = "_MainTex";
    public float angleDegrees = 90f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        Material mat = rend.material;

        float rad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        // Ruota intorno al centro (0.5, 0.5)
        Vector2 center = new Vector2(0.5f, 0.5f);
        Vector2 offset = center - new Vector2(
            center.x * cos - center.y * sin,
            center.x * sin + center.y * cos
        );

        mat.SetTextureOffset(texturePropertyName, offset);

   
        transform.Rotate(0f, 0f, angleDegrees);
    }
}