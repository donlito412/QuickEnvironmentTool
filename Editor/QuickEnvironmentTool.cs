using UnityEngine;
using UnityEditor;

/// <summary>
/// Quick Environment Tool - One-Click World Generation
/// Version 1.0 | Instant Game Environments
/// </summary>
public class QuickEnvironmentTool : EditorWindow
{
    public enum EnvironmentStyle { Forest, Desert, Snow, Tropical, Volcanic }
    public enum TimeOfDay { Dawn, Noon, Sunset, Night }
    public enum WorldSize { Small, Medium, Large, Huge }
    
    private EnvironmentStyle style = EnvironmentStyle.Forest;
    private TimeOfDay timeOfDay = TimeOfDay.Noon;
    private WorldSize worldSize = WorldSize.Medium;
    
    private bool addWater = true;
    private bool addTrees = true;
    private bool addProps = true;
    
    [MenuItem("Tools/Quick Environment")]
    public static void ShowWindow()
    {
        QuickEnvironmentTool window = GetWindow<QuickEnvironmentTool>("Quick Environment");
        window.minSize = new Vector2(350, 400);
    }
    
    private void OnGUI()
    {
        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 18;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.Space(10);
        GUILayout.Label("⚡ QUICK ENVIRONMENT", headerStyle);
        GUILayout.Label("One-Click World Generation", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(15);
        
        // Style Selection
        GUILayout.Label("🎨 Environment Style", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        style = (EnvironmentStyle)EditorGUILayout.EnumPopup("Style", style);
        DrawStylePreview();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Settings
        GUILayout.Label("⚙️ Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        worldSize = (WorldSize)EditorGUILayout.EnumPopup("World Size", worldSize);
        timeOfDay = (TimeOfDay)EditorGUILayout.EnumPopup("Time of Day", timeOfDay);
        EditorGUILayout.Space(5);
        addWater = EditorGUILayout.Toggle("Add Water", addWater);
        addTrees = EditorGUILayout.Toggle("Add Trees", addTrees);
        addProps = EditorGUILayout.Toggle("Add Props", addProps);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        
        // Generate Button
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("🌍 GENERATE WORLD", GUILayout.Height(50)))
        {
            GenerateWorld();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Clear Button
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("🗑️ Clear World", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear World", "Delete all generated objects?", "Yes", "Cancel"))
            {
                ClearWorld();
            }
        }
        GUI.backgroundColor = Color.white;
    }
    
    private void DrawStylePreview()
    {
        string desc = style switch
        {
            EnvironmentStyle.Forest => "🌲 Green hills, lush trees, rivers",
            EnvironmentStyle.Desert => "🏜️ Sandy dunes, cacti, oases",
            EnvironmentStyle.Snow => "❄️ Snowy peaks, pine trees, frozen lakes",
            EnvironmentStyle.Tropical => "🌴 Beaches, palm trees, ocean",
            EnvironmentStyle.Volcanic => "🌋 Dark terrain, lava pools, rocks",
            _ => ""
        };
        EditorGUILayout.HelpBox(desc, MessageType.None);
    }
    
    private void GenerateWorld()
    {
        EditorUtility.DisplayProgressBar("Quick Environment", "Creating terrain...", 0.1f);
        
        // Get size
        int size = worldSize switch
        {
            WorldSize.Small => 250,
            WorldSize.Medium => 500,
            WorldSize.Large => 1000,
            WorldSize.Huge => 2000,
            _ => 500
        };
        
        // Create terrain
        CreateTerrain(size);
        
        EditorUtility.DisplayProgressBar("Quick Environment", "Shaping terrain...", 0.3f);
        ShapeTerrain();
        
        if (addWater)
        {
            EditorUtility.DisplayProgressBar("Quick Environment", "Adding water...", 0.5f);
            AddWater(size);
        }
        
        if (addTrees)
        {
            EditorUtility.DisplayProgressBar("Quick Environment", "Planting trees...", 0.7f);
            AddTrees();
        }
        
        if (addProps)
        {
            EditorUtility.DisplayProgressBar("Quick Environment", "Scattering props...", 0.85f);
            AddProps();
        }
        
        EditorUtility.DisplayProgressBar("Quick Environment", "Setting up lighting...", 0.95f);
        SetupLighting();
        
        EditorUtility.ClearProgressBar();
        Debug.Log("✅ World generated: " + style + " environment!");
    }
    
    private void CreateTerrain(int size)
    {
        Terrain existing = FindFirstObjectByType<Terrain>();
        if (existing != null) DestroyImmediate(existing.gameObject);
        
        TerrainData data = new TerrainData();
        data.heightmapResolution = 513;
        data.size = new Vector3(size, 100, size);
        
        GameObject terrainObj = Terrain.CreateTerrainGameObject(data);
        terrainObj.name = "Terrain";
        terrainObj.transform.position = new Vector3(-size/2, 0, -size/2);
    }
    
    private void ShapeTerrain()
    {
        Terrain terrain = FindFirstObjectByType<Terrain>();
        if (terrain == null) return;
        
        TerrainData data = terrain.terrainData;
        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];
        
        float scale = style switch
        {
            EnvironmentStyle.Desert => 0.008f,
            EnvironmentStyle.Snow => 0.015f,
            EnvironmentStyle.Volcanic => 0.02f,
            _ => 0.01f
        };
        
        float heightMult = style switch
        {
            EnvironmentStyle.Desert => 0.15f,
            EnvironmentStyle.Snow => 0.5f,
            EnvironmentStyle.Volcanic => 0.4f,
            EnvironmentStyle.Tropical => 0.1f,
            _ => 0.25f
        };
        
        float seed = Random.Range(0f, 1000f);
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                heights[x, y] = Mathf.PerlinNoise(x * scale + seed, y * scale + seed) * heightMult;
            }
        }
        
        data.SetHeights(0, 0, heights);
    }
    
    private void AddWater(int size)
    {
        GameObject existing = GameObject.Find("Water");
        if (existing != null) DestroyImmediate(existing);
        
        float level = style switch
        {
            EnvironmentStyle.Desert => 2f,
            EnvironmentStyle.Snow => 8f,
            EnvironmentStyle.Volcanic => 5f,
            _ => 10f
        };
        
        Color waterColor = style switch
        {
            EnvironmentStyle.Volcanic => new Color(0.8f, 0.2f, 0.1f, 0.9f),
            EnvironmentStyle.Tropical => new Color(0.1f, 0.6f, 0.8f, 0.8f),
            EnvironmentStyle.Snow => new Color(0.7f, 0.85f, 0.95f, 0.7f),
            _ => new Color(0.2f, 0.5f, 0.7f, 0.8f)
        };
        
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0, level, 0);
        water.transform.localScale = new Vector3(size / 8f, 1, size / 8f);
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = waterColor;
        water.GetComponent<Renderer>().material = mat;
        DestroyImmediate(water.GetComponent<Collider>());
    }
    
    private void AddTrees()
    {
        Terrain terrain = FindFirstObjectByType<Terrain>();
        if (terrain == null) return;
        
        GameObject parent = GameObject.Find("Trees") ?? new GameObject("Trees");
        
        Color trunkColor = new Color(0.4f, 0.25f, 0.15f);
        Color leafColor = style switch
        {
            EnvironmentStyle.Snow => new Color(0.15f, 0.3f, 0.15f),
            EnvironmentStyle.Desert => new Color(0.4f, 0.5f, 0.2f),
            EnvironmentStyle.Volcanic => new Color(0.1f, 0.1f, 0.1f),
            EnvironmentStyle.Tropical => new Color(0.1f, 0.5f, 0.1f),
            _ => new Color(0.2f, 0.5f, 0.2f)
        };
        
        Material trunk = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        trunk.color = trunkColor;
        Material leaves = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        leaves.color = leafColor;
        
        TerrainData data = terrain.terrainData;
        Vector3 pos = terrain.transform.position;
        int count = (int)(data.size.x * 0.5f);
        
        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = new Vector3(
                pos.x + Random.value * data.size.x,
                0,
                pos.z + Random.value * data.size.z
            );
            worldPos.y = terrain.SampleHeight(worldPos);
            
            if (worldPos.y < 12f) continue;
            
            float scale = Random.Range(0.8f, 1.5f);
            
            GameObject tree = new GameObject("Tree");
            tree.transform.parent = parent.transform;
            tree.transform.position = worldPos;
            
            GameObject trunkObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunkObj.transform.parent = tree.transform;
            trunkObj.transform.localPosition = new Vector3(0, 2f * scale, 0);
            trunkObj.transform.localScale = new Vector3(0.3f * scale, 2f * scale, 0.3f * scale);
            trunkObj.GetComponent<Renderer>().material = trunk;
            DestroyImmediate(trunkObj.GetComponent<Collider>());
            
            GameObject leavesObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leavesObj.transform.parent = tree.transform;
            leavesObj.transform.localPosition = new Vector3(0, 4.5f * scale, 0);
            leavesObj.transform.localScale = Vector3.one * 3f * scale;
            leavesObj.GetComponent<Renderer>().material = leaves;
            DestroyImmediate(leavesObj.GetComponent<Collider>());
        }
    }
    
    private void AddProps()
    {
        Terrain terrain = FindFirstObjectByType<Terrain>();
        if (terrain == null) return;
        
        GameObject parent = GameObject.Find("Props") ?? new GameObject("Props");
        
        Color propColor = style switch
        {
            EnvironmentStyle.Desert => new Color(0.8f, 0.7f, 0.5f),
            EnvironmentStyle.Snow => new Color(0.9f, 0.9f, 0.95f),
            EnvironmentStyle.Volcanic => new Color(0.2f, 0.15f, 0.15f),
            _ => new Color(0.4f, 0.4f, 0.4f)
        };
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = propColor;
        
        TerrainData data = terrain.terrainData;
        Vector3 pos = terrain.transform.position;
        int count = (int)(data.size.x * 0.2f);
        
        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = new Vector3(
                pos.x + Random.value * data.size.x,
                0,
                pos.z + Random.value * data.size.z
            );
            worldPos.y = terrain.SampleHeight(worldPos) + 0.3f;
            
            GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prop.name = "Rock";
            prop.transform.parent = parent.transform;
            prop.transform.position = worldPos;
            prop.transform.localScale = Vector3.one * Random.Range(0.3f, 1.5f);
            prop.GetComponent<Renderer>().material = mat;
            DestroyImmediate(prop.GetComponent<Collider>());
        }
    }
    
    private void SetupLighting()
    {
        Light sun = FindFirstObjectByType<Light>();
        if (sun == null)
        {
            GameObject obj = new GameObject("Sun");
            sun = obj.AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        
        (float rot, Color col, float intensity) = timeOfDay switch
        {
            TimeOfDay.Dawn => (5f, new Color(1f, 0.7f, 0.5f), 0.6f),
            TimeOfDay.Noon => (50f, new Color(1f, 0.95f, 0.9f), 1.2f),
            TimeOfDay.Sunset => (10f, new Color(1f, 0.5f, 0.3f), 0.7f),
            TimeOfDay.Night => (-30f, new Color(0.3f, 0.3f, 0.5f), 0.15f),
            _ => (50f, Color.white, 1f)
        };
        
        sun.transform.rotation = Quaternion.Euler(rot, -30, 0);
        sun.color = col;
        sun.intensity = intensity;
        sun.shadows = LightShadows.Soft;
    }
    
    private void ClearWorld()
    {
        Terrain terrain = FindFirstObjectByType<Terrain>();
        if (terrain != null) DestroyImmediate(terrain.gameObject);
        
        string[] toDelete = { "Water", "Trees", "Props", "Sun" };
        foreach (string name in toDelete)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }
        
        Debug.Log("✅ World cleared!");
    }
}
