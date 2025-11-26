using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResetPSXGlobals : MonoBehaviour
{
    private static readonly string[] PSX_KEYWORDS = new string[] {
        "PSX_ENABLE_CUSTOM_VERTEX_LIGHTING",
        "PSX_FLAT_SHADING_MODE_CENTER",
        "PSX_TRIANGLE_SORT_OFF",
        "PSX_TRIANGLE_SORT_CENTER_Z",
        "PSX_TRIANGLE_SORT_CLOSEST_Z",
        "PSX_TRIANGLE_SORT_CENTER_VIEWDIST",
        "PSX_TRIANGLE_SORT_CLOSEST_VIEWDIST",
        "PSX_TRIANGLE_SORT_CUSTOM"
    };

    private static readonly (string name, float value)[] PSX_GLOBAL_FLOATS = new (string, float)[] {
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

    [ContextMenu("Reset PSX Globals Now")]
    public void ResetNow()
    {
        foreach (var k in PSX_KEYWORDS)
        {
            Shader.DisableKeyword(k);
            Debug.Log("[ResetPSX] Disabled keyword: " + k);
        }

        foreach (var kv in PSX_GLOBAL_FLOATS)
        {
            Shader.SetGlobalFloat(kv.name, kv.value);
            Debug.Log($"[ResetPSX] Shader.SetGlobalFloat({kv.name}, {kv.value})");
        }

        var allMono = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        foreach (var m in allMono)
        {
            if (m == null) continue;
            var t = m.GetType();
            if (t.Name == "PSXPostProcessEffect" || t.FullName != null && t.FullName.Contains("PSXShaderKit.PSXPostProcessEffect"))
            {
                var go = m.gameObject;
                Debug.Log($"[ResetPSX] Found {t.Name} on '{go.name}' - destroying component.");
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(m);
                else
#endif
                    Destroy(m);
            }
        }

        foreach (var m in allMono)
        {
            if (m == null) continue;
            var t = m.GetType();
            if (t.Name == "PSXShaderManager" || t.FullName != null && t.FullName.Contains("PSXShaderKit.PSXShaderManager"))
            {
                var go = m.gameObject;
                Debug.Log($"[ResetPSX] Found {t.Name} on '{go.name}' - destroying component.");
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(m);
                else
#endif
                    Destroy(m);
            }
        }

        Debug.Log("[ResetPSX] Finished clearing PSX globals and components.");
    }

    private void Start()
    {
        ResetNow();
    }
}
