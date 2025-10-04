// WebGLFrameSync.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro; // Only compiles if TextMeshPro is in the project
#endif

/// <summary>
/// Utilities for doing something "next frame" in a way that behaves well on WebGL
/// (where requestAnimationFrame, canvas sizing, and font loading can delay the real render).
/// </summary>
public static class WebGLFrameSync
{
    // --------------------------
    // 1) Basic "next frame"
    // --------------------------

    /// <summary>Run after exactly 1 Update tick.</summary>
    public static Coroutine NextFrame(MonoBehaviour host, Action action)
        => host.StartCoroutine(CoNextFrames(1, action));

    /// <summary>Run after N Update ticks (at least 1).</summary>
    public static Coroutine NextFrames(MonoBehaviour host, int frames, Action action)
        => host.StartCoroutine(CoNextFrames(frames, action));

    static IEnumerator CoNextFrames(int frames, Action action)
    {
        for (int i = 0; i < Mathf.Max(1, frames); i++)
            yield return null;
        action?.Invoke();
    }

    // --------------------------
    // 2) After the NEXT UI render (robust on WebGL)
    // --------------------------

    /// <summary>
    /// Runs after the next time Unity is about to render canvases, then one more Update tick,
    /// so layout/sizing is actually stable. Great for reading RectTransforms on WebGL.
    /// </summary>
    public static Coroutine AfterNextRenderedFrame(MonoBehaviour host, Action action)
        => host.StartCoroutine(CoAfterNextRenderedFrame(action));

    static IEnumerator CoAfterNextRenderedFrame(Action action)
    {
        bool fired = false;
        Canvas.WillRenderCanvases cb = () => fired = true;
        Canvas.willRenderCanvases += cb;

        // Wait until the next render of UI actually occurs
        yield return new WaitUntil(() => fired);
        Canvas.willRenderCanvases -= cb;

        // Step once more so we're safely after that render
        yield return null;

        action?.Invoke();
    }

    // --------------------------
    // 3) End of next frame (after rendering)
    // --------------------------

    /// <summary>Runs at the very end of the next frame (after rendering).</summary>
    public static Coroutine AfterEndOfNextFrame(MonoBehaviour host, Action action)
        => host.StartCoroutine(CoAfterEndOfNextFrame(action));

    static IEnumerator CoAfterEndOfNextFrame(Action action)
    {
        yield return null;                   // next Update
        yield return new WaitForEndOfFrame();// then end-of-frame
        action?.Invoke();
    }

    // --------------------------
    // 4) When screen size has stabilized (handles async WebGL canvas/DPR changes)
    // --------------------------

    /// <summary>
    /// Waits until Screen.width/height are unchanged for a number of consecutive frames,
    /// then runs. Useful right after scene load or on orientation/device-pixel-ratio changes.
    /// </summary>
    public static Coroutine WhenScreenStable(MonoBehaviour host, Action action, int stableFrames = 2)
        => host.StartCoroutine(CoWhenScreenStable(action, stableFrames));

    static IEnumerator CoWhenScreenStable(Action action, int stableFrames)
    {
        int stable = 0;
        Vector2 prev = new Vector2(Screen.width, Screen.height);

        while (stable < Mathf.Max(1, stableFrames))
        {
            yield return null; // next real frame
            Vector2 now = new Vector2(Screen.width, Screen.height);
            if (now == prev) stable++; else { stable = 0; prev = now; }
        }
        action?.Invoke();
    }

    // --------------------------
    // 5) Layout helpers (UI / TMP)
    // --------------------------

    /// <summary>
    /// Forces a layout rebuild after the next UI render, then runs. Good for ContentSizeFitter, LayoutGroups, etc.
    /// </summary>
    public static Coroutine AfterLayoutReady(MonoBehaviour host, RectTransform root, Action action)
        => host.StartCoroutine(CoAfterLayoutReady(root, action));

    static IEnumerator CoAfterLayoutReady(RectTransform root, Action action)
    {
        yield return CoAfterNextRenderedFrame(null); // wait for a real render tick
        if (root) LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        yield return null; // let the rebuild propagate
        action?.Invoke();
    }

#if TMP_PRESENT
    /// <summary>
    /// Ensures TextMeshPro has generated geometry and layout is updated on WebGL before running.
    /// </summary>
    public static Coroutine AfterTMPReady(MonoBehaviour host, TMP_Text text, Action action)
        => host.StartCoroutine(CoAfterTMPReady(text, action));

    static IEnumerator CoAfterTMPReady(TMP_Text text, Action action)
    {
        yield return CoAfterNextRenderedFrame(null);
        if (text) text.ForceMeshUpdate();
        var rt = text ? (RectTransform)text.transform : null;
        if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        yield return null;
        action?.Invoke();
    }
#endif
}
