using UnityEngine;

public class FloatBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 6f;
    [SerializeField] private float speed = 1.5f;

    private RectTransform rt;
    private Vector2 startPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed) * amplitude;
        rt.anchoredPosition = startPos + new Vector2(0, y);
    }
}