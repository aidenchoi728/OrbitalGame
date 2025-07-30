using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIDrifter : MonoBehaviour
{
    public RectTransform rectTransform;
    public float startX;
    public float startY;
    public float driftSpeed = 50f;
    public float noiseScale = 0.5f;

    private Vector2 noiseOffset;

    void Start()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        rectTransform.position = new Vector3(startX, startY, 0);

        noiseOffset = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));
    }

    void Update()
    {
        float time = Time.time * noiseScale;
        float dx = Mathf.PerlinNoise(noiseOffset.x, time) * 2 - 1;
        float dy = Mathf.PerlinNoise(noiseOffset.y, time) * 2 - 1;

        Vector2 driftDirection = new Vector2(dx, dy).normalized;
        Vector2 newPos = new Vector2(rectTransform.position.x, rectTransform.position.y) + driftDirection * driftSpeed * Time.deltaTime;

        // Keep it within the screen bounds (optional clamp)
        Vector2 canvasSize = ((RectTransform)rectTransform.parent).rect.size;
        Vector2 imageSize = rectTransform.rect.size;
        Vector2 halfSize = imageSize * 0.5f;

        newPos.x = Mathf.Clamp(newPos.x, -canvasSize.x / 2 + halfSize.x, canvasSize.x / 2 - halfSize.x);
        newPos.y = Mathf.Clamp(newPos.y, -canvasSize.y / 2 + halfSize.y, canvasSize.y / 2 - halfSize.y);

        rectTransform.position = newPos;
    }
}
