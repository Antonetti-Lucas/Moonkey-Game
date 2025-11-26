#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class PSXResetEditor
{
    [MenuItem("PSX/Reset PSX Globals")]
    public static void ResetPSX()
    {
        // Keywords
        string[] keys = new string[] {
            "PSX_ENABLE_CUSTOM_VERTEX_LIGHTING",
            "PSX_FLAT_SHADING_MODE_CENTER",
            "PSX_TRIANGLE_SORT_OFF",
            "PSX_TRIANGLE_SORT_CENTER_Z",
            "PSX_TRIANGLE_SORT_CLOSEST_Z",
            "PSX_TRIANGLE_SORT_CENTER_VIEWDIST",
            "PSX_TRIANGLE_SORT_CLOSEST_VIEWDIST",
            "PSX_TRIANGLE_SORT_CUSTOM"
        };

        foreach (var k in keys)
        {
            Shader.DisableKeyword(k);
            Debug.Log("[PSX Reset] Disabled: " + k);
        }

        // Global floats
        (string, float)[] globals = new (string, float)[] {
            ("_PSX_GridSize", 100.0f),
            ("_PSX_DepthDebug", 0.0f),
            ("_PSX_LightingNormalFactor", 0.0f),
            ("_PSX_TextureWarpingFactor", 0.0f),
            ("_PSX_LightFalloffPercent", 0.0f),
            ("_PSX_FlatShadingMode", 0.0f),
            ("_PSX_TextureWarpingMode", 0.0f),
            ("_PSX_VertexWobbleMode", 0.0f),
            ("_PSX_ObjectDithering", 0.0f)
        };

        foreach (var g in globals)
        {
            Shader.SetGlobalFloat(g.Item1, g.Item2);
            Debug.Log($"[PSX Reset] SetGlobalFloat {g.Item1} = {g.Item2}");
        }

        // remove PSX components if any exist in scenes/assets (careful)
        var allMono = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        foreach (var m in allMono)
        {
            if (m == null) continue;
            var t = m.GetType().Name;
            if (t == "PSXPostProcessEffect" || t == "PSXShaderManager")
            {
                Debug.Log($"[PSX Reset] Found {t} on '{m.gameObject.name}'. Consider removing it manually if you don't want it.");
            }
        }

        Debug.Log("[PSX Reset] Done. If you still see effects, save scenes and restart the Editor (this definitely clears any leftover state).");
    }
}
#endif
