using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Linq;

enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full = 0, Half, Quarter, Eighth, Sixteenth }

class ExportTerrain : EditorWindow
{
    SaveFormat saveFormat = SaveFormat.Triangles;
    SaveResolution saveResolution = SaveResolution.Half;
    bool exportTextures = true;
    int textureResolution = 2048;

    static TerrainData terrain;
    static Vector3 terrainPos;

    int tCount;
    int counter;
    int totalCount;
    int progressUpdateInterval = 10000;

    [MenuItem("Terrain/Export To Obj...")]
    static void Init()
    {
        terrain = null;
        Terrain terrainObject = Selection.activeObject as Terrain;
        if (!terrainObject)
        {
            terrainObject = Terrain.activeTerrain;
        }
        if (terrainObject)
        {
            terrain = terrainObject.terrainData;
            terrainPos = terrainObject.transform.position;
        }

        EditorWindow.GetWindow<ExportTerrain>().Show();
    }

    void OnGUI()
    {
        if (!terrain)
        {
            GUILayout.Label("No terrain found");
            if (GUILayout.Button("Cancel"))
            {
                EditorWindow.GetWindow<ExportTerrain>().Close();
            }
            return;
        }

        saveFormat = (SaveFormat)EditorGUILayout.EnumPopup("Export Format", saveFormat);
        saveResolution = (SaveResolution)EditorGUILayout.EnumPopup("Resolution", saveResolution);
        exportTextures = EditorGUILayout.Toggle("Export Textures", exportTextures);

        if (exportTextures)
        {
            textureResolution = EditorGUILayout.IntField("Texture Resolution", textureResolution);
            textureResolution = Mathf.Clamp(textureResolution, 512, 8192);
        }

        if (GUILayout.Button("Export"))
        {
            Export();
        }
    }

    void Export()
    {
        string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
        if (string.IsNullOrEmpty(fileName))
            return;

        string directory = Path.GetDirectoryName(fileName);
        string baseName = Path.GetFileNameWithoutExtension(fileName);

        int terrainWidth = terrain.heightmapResolution;
        int terrainHeight = terrain.heightmapResolution;
        Vector3 terrainSize = terrain.size;

        int tRes = (int)Mathf.Pow(2, (int)saveResolution);

        // Calculate the actual mesh dimensions after resolution reduction
        int w = (terrainWidth - 1) / tRes + 1;
        int h = (terrainHeight - 1) / tRes + 1;

        // Correct mesh scale - account for resolution reduction
        Vector3 meshScale = new Vector3(
            terrainSize.x / (terrainWidth - 1) * tRes,
            terrainSize.y,
            terrainSize.z / (terrainHeight - 1) * tRes
        );

        float[,] heightData = terrain.GetHeights(0, 0, terrainWidth, terrainHeight);

        Vector3[] vertices = new Vector3[w * h];
        Vector2[] uvs = new Vector2[w * h];

        int[] triangles;

        if (saveFormat == SaveFormat.Triangles)
        {
            triangles = new int[(w - 1) * (h - 1) * 6];
        }
        else
        {
            triangles = new int[(w - 1) * (h - 1) * 4];
        }

        // Build vertices and UVs with correct coordinate mapping
        for (int z = 0; z < h; z++)
        {
            for (int x = 0; x < w; x++)
            {
                // Sample height from original terrain data with correct coordinates
                int sampleX = x * tRes;
                int sampleZ = z * tRes;
                sampleX = Mathf.Clamp(sampleX, 0, terrainWidth - 1);
                sampleZ = Mathf.Clamp(sampleZ, 0, terrainHeight - 1);

                float height = heightData[sampleZ, sampleX];

                // Correct vertex position - match Unity's coordinate system
                vertices[z * w + x] = new Vector3(
                    x * meshScale.x,                    // X position
                    height * meshScale.y,               // Y position (height)
                    z * meshScale.z                     // Z position
                ) + terrainPos;

                // Correct UV coordinates
                uvs[z * w + x] = new Vector2(
                    x / (float)(w - 1),
                    z / (float)(h - 1)
                );
            }
        }

        // Build triangles with correct winding order
        int index = 0;
        if (saveFormat == SaveFormat.Triangles)
        {
            for (int z = 0; z < h - 1; z++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    int bottomLeft = z * w + x;
                    int bottomRight = z * w + x + 1;
                    int topLeft = (z + 1) * w + x;
                    int topRight = (z + 1) * w + x + 1;

                    // First triangle - counter-clockwise winding
                    triangles[index++] = bottomLeft;
                    triangles[index++] = topLeft;
                    triangles[index++] = bottomRight;

                    // Second triangle - counter-clockwise winding
                    triangles[index++] = bottomRight;
                    triangles[index++] = topLeft;
                    triangles[index++] = topRight;
                }
            }
        }
        else
        {
            for (int z = 0; z < h - 1; z++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    int bottomLeft = z * w + x;
                    int topLeft = (z + 1) * w + x;
                    int topRight = (z + 1) * w + x + 1;
                    int bottomRight = z * w + x + 1;

                    // Quad - counter-clockwise winding
                    triangles[index++] = bottomLeft;
                    triangles[index++] = topLeft;
                    triangles[index++] = topRight;
                    triangles[index++] = bottomRight;
                }
            }
        }

        // Export textures if enabled
        string materialFileName = null;
        if (exportTextures)
        {
            materialFileName = ExportTerrainTextures(directory, baseName);
        }

        // Export to .obj
        using (StreamWriter sw = new StreamWriter(fileName))
        {
            try
            {
                sw.WriteLine("# Unity terrain OBJ File");
                sw.WriteLine("# Terrain Size: " + terrainSize);
                sw.WriteLine("# Vertex Count: " + vertices.Length);
                sw.WriteLine("# Face Count: " + (saveFormat == SaveFormat.Triangles ? triangles.Length / 3 : triangles.Length / 4));

                // Write material reference if textures were exported
                if (!string.IsNullOrEmpty(materialFileName))
                {
                    sw.WriteLine("mtllib " + Path.GetFileName(materialFileName));
                }

                // Write vertices
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                counter = tCount = 0;
                totalCount = (vertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? triangles.Length / 3 : triangles.Length / 4)) / progressUpdateInterval;

                for (int i = 0; i < vertices.Length; i++)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("v ", 20);
                    sb.Append(vertices[i].x.ToString("F6")).Append(" ").
                       Append(vertices[i].y.ToString("F6")).Append(" ").
                       Append(vertices[i].z.ToString("F6"));
                    sw.WriteLine(sb);
                }

                // Write UVs
                for (int i = 0; i < uvs.Length; i++)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("vt ", 22);
                    sb.Append(uvs[i].x.ToString("F6")).Append(" ").
                       Append(uvs[i].y.ToString("F6"));
                    sw.WriteLine(sb);
                }

                // Write material usage
                if (!string.IsNullOrEmpty(materialFileName))
                {
                    sw.WriteLine("usemtl TerrainMaterial");
                }

                // Write faces with correct OBJ format (1-based indexing)
                if (saveFormat == SaveFormat.Triangles)
                {
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        UpdateProgress();
                        // OBJ uses 1-based indexing, so add 1 to each index
                        int v1 = triangles[i] + 1;
                        int v2 = triangles[i + 1] + 1;
                        int v3 = triangles[i + 2] + 1;

                        StringBuilder sb = new StringBuilder("f ", 43);
                        sb.Append(v1).Append("/").Append(v1).Append(" ").
                           Append(v2).Append("/").Append(v2).Append(" ").
                           Append(v3).Append("/").Append(v3);
                        sw.WriteLine(sb);
                    }
                }
                else
                {
                    for (int i = 0; i < triangles.Length; i += 4)
                    {
                        UpdateProgress();
                        // OBJ uses 1-based indexing, so add 1 to each index
                        int v1 = triangles[i] + 1;
                        int v2 = triangles[i + 1] + 1;
                        int v3 = triangles[i + 2] + 1;
                        int v4 = triangles[i + 3] + 1;

                        StringBuilder sb = new StringBuilder("f ", 57);
                        sb.Append(v1).Append("/").Append(v1).Append(" ").
                           Append(v2).Append("/").Append(v2).Append(" ").
                           Append(v3).Append("/").Append(v3).Append(" ").
                           Append(v4).Append("/").Append(v4);
                        sw.WriteLine(sb);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.LogError("Error saving file: " + err.Message);
                EditorUtility.DisplayDialog("Export Error", "Failed to export terrain: " + err.Message, "OK");
            }
        }

        terrain = null;
        EditorUtility.ClearProgressBar();
        EditorWindow.GetWindow<ExportTerrain>().Close();

        // Show completion message
        EditorUtility.DisplayDialog("Export Complete",
            "Terrain exported successfully!\n\nFiles created:\n" +
            Path.GetFileName(fileName) + "\n" +
            (exportTextures && !string.IsNullOrEmpty(materialFileName) ? Path.GetFileName(materialFileName) + "\n+ texture files" : ""),
            "OK");
    }

    string ExportTerrainTextures(string directory, string baseName)
    {
        try
        {
            Terrain currentTerrain = Selection.activeObject as Terrain;
            if (!currentTerrain) currentTerrain = Terrain.activeTerrain;
            if (!currentTerrain) return null;

            string materialPath = Path.Combine(directory, baseName + ".mtl");
            using (StreamWriter mtlWriter = new StreamWriter(materialPath))
            {
                mtlWriter.WriteLine("# Terrain Material File");
                mtlWriter.WriteLine("newmtl TerrainMaterial");
                mtlWriter.WriteLine("Ka 1.000 1.000 1.000"); // Ambient
                mtlWriter.WriteLine("Kd 1.000 1.000 1.000"); // Diffuse
                mtlWriter.WriteLine("Ks 0.000 0.000 0.000"); // Specular
                mtlWriter.WriteLine("Ns 0.000"); // Specular exponent
                mtlWriter.WriteLine("illum 2");

                // Generate composite texture from all terrain layers
                string compositeTexturePath = GenerateCompositeTexture(currentTerrain, directory, baseName);
                if (!string.IsNullOrEmpty(compositeTexturePath))
                {
                    mtlWriter.WriteLine("map_Kd " + Path.GetFileName(compositeTexturePath));
                }
            }

            return materialPath;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to export textures: " + e.Message);
            return null;
        }
    }

    string GenerateCompositeTexture(Terrain terrain, string directory, string baseName)
    {
        try
        {
            TerrainData terrainData = terrain.terrainData;

            if (terrainData.terrainLayers == null || terrainData.terrainLayers.Length == 0)
            {
                Debug.LogWarning("No terrain layers found");
                return null;
            }

            // Get alphamaps (splat maps)
            float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            int alphaWidth = terrainData.alphamapWidth;
            int alphaHeight = terrainData.alphamapHeight;

            // Create readable copies of all terrain layer textures
            Texture2D[] readableTextures = new Texture2D[terrainData.terrainLayers.Length];
            for (int i = 0; i < terrainData.terrainLayers.Length; i++)
            {
                if (terrainData.terrainLayers[i] != null && terrainData.terrainLayers[i].diffuseTexture != null)
                {
                    readableTextures[i] = CreateReadableTexture(terrainData.terrainLayers[i].diffuseTexture);
                }
            }

            // Create composite texture
            Texture2D compositeTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, false);

            // Scale factor from alphamap resolution to output texture resolution
            float scaleX = (float)alphaWidth / textureResolution;
            float scaleY = (float)alphaHeight / textureResolution;

            EditorUtility.DisplayProgressBar("Generating Composite Texture", "Blending terrain layers...", 0f);

            for (int y = 0; y < textureResolution; y++)
            {
                if (y % 100 == 0)
                {
                    EditorUtility.DisplayProgressBar("Generating Composite Texture", "Blending terrain layers...", (float)y / textureResolution);
                }

                for (int x = 0; x < textureResolution; x++)
                {
                    // Sample alphamap
                    int alphaX = Mathf.FloorToInt(x * scaleX);
                    int alphaY = Mathf.FloorToInt(y * scaleY);
                    alphaX = Mathf.Clamp(alphaX, 0, alphaWidth - 1);
                    alphaY = Mathf.Clamp(alphaY, 0, alphaHeight - 1);

                    Color finalColor = Color.clear;

                    // Blend all layers based on alphamap values
                    for (int layer = 0; layer < terrainData.terrainLayers.Length && layer < alphamaps.GetLength(2); layer++)
                    {
                        if (readableTextures[layer] == null)
                            continue;

                        float alpha = alphamaps[alphaY, alphaX, layer];

                        if (alpha > 0.001f)
                        {
                            // Sample the layer texture
                            Texture2D diffuseTex = readableTextures[layer];
                            TerrainLayer terrainLayer = terrainData.terrainLayers[layer];

                            Vector2 texCoord = new Vector2(
                                (x / (float)textureResolution) * terrainData.size.x / terrainLayer.tileSize.x,
                                (y / (float)textureResolution) * terrainData.size.z / terrainLayer.tileSize.y
                            );

                            Color layerColor = SampleTextureBilinear(diffuseTex, texCoord);
                            finalColor = Color.Lerp(finalColor, layerColor, alpha);
                        }
                    }

                    // If no layers contributed, use a fallback color
                    if (finalColor.a < 0.01f)
                    {
                        finalColor = Color.gray;
                    }

                    compositeTexture.SetPixel(x, y, finalColor);
                }
            }

            compositeTexture.Apply();

            // Save the composite texture
            string texturePath = Path.Combine(directory, baseName + "_composite.png");
            byte[] bytes = compositeTexture.EncodeToPNG();
            File.WriteAllBytes(texturePath, bytes);

            // Cleanup
            for (int i = 0; i < readableTextures.Length; i++)
            {
                if (readableTextures[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(readableTextures[i]);
                }
            }
            UnityEngine.Object.DestroyImmediate(compositeTexture);

            EditorUtility.ClearProgressBar();
            return texturePath;
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("Failed to generate composite texture: " + e.Message);
            return null;
        }
    }

    Texture2D CreateReadableTexture(Texture2D source)
    {
        if (source == null) return null;

        // If the texture is already readable, return it as is
        if (source.isReadable) return source;

        try
        {
            // Create a temporary RenderTexture
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Copy the source texture to the RenderTexture
            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            // Create a new readable Texture2D
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();

            // Restore the previous RenderTexture
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return readableText;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create readable texture: " + e.Message);
            return null;
        }
    }

    Color SampleTextureBilinear(Texture2D texture, Vector2 uv)
    {
        if (texture == null) return Color.black;

        // Wrap UV coordinates for tiling
        uv.x = Mathf.Repeat(uv.x, 1f);
        uv.y = Mathf.Repeat(uv.y, 1f);

        // Convert to pixel coordinates
        float x = uv.x * (texture.width - 1);
        float y = uv.y * (texture.height - 1);

        int xFloor = Mathf.FloorToInt(x);
        int yFloor = Mathf.FloorToInt(y);
        int xCeil = Mathf.CeilToInt(x);
        int yCeil = Mathf.CeilToInt(y);

        // Clamp coordinates
        xFloor = Mathf.Clamp(xFloor, 0, texture.width - 1);
        yFloor = Mathf.Clamp(yFloor, 0, texture.height - 1);
        xCeil = Mathf.Clamp(xCeil, 0, texture.width - 1);
        yCeil = Mathf.Clamp(yCeil, 0, texture.height - 1);

        // Get the four surrounding pixels
        Color q11 = texture.GetPixel(xFloor, yFloor);
        Color q12 = texture.GetPixel(xFloor, yCeil);
        Color q21 = texture.GetPixel(xCeil, yFloor);
        Color q22 = texture.GetPixel(xCeil, yCeil);

        // Bilinear interpolation
        float xLerp = x - xFloor;
        float yLerp = y - yFloor;

        Color top = Color.Lerp(q11, q21, xLerp);
        Color bottom = Color.Lerp(q12, q22, xLerp);
        return Color.Lerp(top, bottom, yLerp);
    }

    void UpdateProgress()
    {
        if (counter++ == progressUpdateInterval)
        {
            counter = 0;
            EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
        }
    }
}