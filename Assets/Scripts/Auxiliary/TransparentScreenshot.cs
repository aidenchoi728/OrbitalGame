using UnityEngine;
using System.IO;

public class TransparentScreenshot : MonoBehaviour
{
    [SerializeField] int width = 1024;
    [SerializeField] int height = 1024;

    [ContextMenu("Take Transparent Screenshot")]
    public void TakeScreenshot()
    {
        // Create a temporary RenderTexture with alpha
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Camera cam = Camera.main;

        // Store old settings
        RenderTexture prev = cam.targetTexture;

        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0); // Transparent background

        cam.Render();

        // Read pixels into Texture2D
        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // Save as PNG with alpha
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../TransparentScreenshot.png", bytes);

        // Cleanup
        cam.targetTexture = prev;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(screenshot);

        Debug.Log("Saved Transparent Screenshot with Alpha!");
    }
}