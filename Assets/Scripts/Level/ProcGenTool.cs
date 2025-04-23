using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ProcGenTool : MonoBehaviour
{
    [System.Serializable]
    public class LevelSettings
    {
        public ProcGen.Difficulty difficulty = ProcGen.Difficulty.Hard;
        public int numberOfChunks = 4;
        [Range(0f, 1f)]
        public float spikeSpawnChance = 0f;
        [Range(0f, 1f)]
        public float enemySpawnChance = 0f;
    }

    public LevelSettings settings;
    private ProcGen procGenInstance;

    private void OnEnable()
    {
        FindProcGenInstance();
    }

    [ContextMenu("Generate New Level with Settings")]
    public void GenerateNewLevelFromEditor()
    {
        GenerateLevel();
    }

    [ContextMenu("Clear Level")]
    public void ClearLevelFromEditor()
    {
        ClearLevel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Application.isPlaying && other.CompareTag("Player"))
        {
            GenerateLevel();
        }
    }

    private void GenerateLevel()
    {
        if (FindProcGenInstance() && settings != null)
        {
            // Clear existing level first
            ClearLevel();

            ApplySettingsToProcGen();
            procGenInstance.GenerateNewLevel();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                FixPlatformerAgentReferences();
            }
#endif
        }
        else
        {
            Debug.LogWarning($"Cannot generate level. ProcGen instance: {(procGenInstance != null ? "found" : "missing")}. Settings: {(settings != null ? "set" : "missing")}.");
        }
    }

    private void ClearLevel()
    {
        if (!FindProcGenInstance() || settings == null)
        {
            Debug.LogWarning($"Cannot clear level. ProcGen instance: {(procGenInstance != null ? "found" : "missing")}. Settings: {(settings != null ? "set" : "missing")}.");
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ClearLevelInEditMode();
        }
        else
#endif
        {
            procGenInstance.ClearExistingLevel();
        }
    }

#if UNITY_EDITOR
    private void ClearLevelInEditMode()
    {
        // Clear tilemaps
        procGenInstance.groundTilemap.ClearAllTiles();
        procGenInstance.hazardTilemap.ClearAllTiles();
        procGenInstance.wallTilemap.ClearAllTiles();

        // Collect all children into a list to avoid iterator issues
        Transform[] children = new Transform[procGenInstance.transform.childCount];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = procGenInstance.transform.GetChild(i);
        }

        // Destroy all collected children
        foreach (Transform child in children)
        {
            if (child != null) // Ensure the child still exists
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        // Clear the generatedObjects list in ProcGen to keep it in sync
        System.Reflection.FieldInfo generatedObjectsField = typeof(ProcGen).GetField("generatedObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (generatedObjectsField != null)
        {
            System.Collections.Generic.List<GameObject> generatedObjects = (System.Collections.Generic.List<GameObject>)generatedObjectsField.GetValue(procGenInstance);
            if (generatedObjects != null)
            {
                generatedObjects.Clear();
            }
        }

        Debug.Log("Level cleared in Edit Mode. All children of ProcGen GameObject have been destroyed.");
    }

    private void FixPlatformerAgentReferences()
    {
        GameObject flag = GameObject.Find("Flag");
        if (flag == null)
        {
            Debug.LogWarning("Flag object not found after level generation. PlatformerAgent goalTransform may not be set.");
            return;
        }

        PlatformerAgent[] agents = FindObjectsOfType<PlatformerAgent>();
        foreach (PlatformerAgent agent in agents)
        {
            if (agent.goalTransform == null)
            {
                agent.goalTransform = flag.transform;
                EditorUtility.SetDirty(agent);
            }
        }
    }
#endif

    private bool FindProcGenInstance()
    {
        if (ProcGen.Instance != null)
        {
            procGenInstance = ProcGen.Instance;
            return true;
        }

        procGenInstance = FindObjectOfType<ProcGen>();
        if (procGenInstance != null)
        {
            Debug.Log("Found ProcGen component in scene via FindObjectOfType.");
            return true;
        }

        // Adjust "ProcGenObject" to match your GameObject's name
        GameObject procGenObject = GameObject.Find("ProcGenObject");
        if (procGenObject != null)
        {
            procGenInstance = procGenObject.GetComponent<ProcGen>();
            if (procGenInstance != null)
            {
                Debug.Log($"Found ProcGen component on GameObject '{procGenObject.name}'.");
                return true;
            }
        }

        Debug.LogWarning("ProcGen instance not found in the scene. Ensure a GameObject with ProcGen component is active.");
        return false;
    }

    private void ApplySettingsToProcGen()
    {
        procGenInstance.currentDifficulty = settings.difficulty;
        procGenInstance.numberOfChunks = settings.numberOfChunks;
        procGenInstance.spikeSpawnChance = settings.spikeSpawnChance;
        procGenInstance.EnemySpawnChance = settings.enemySpawnChance;
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Generate Level with ProcGenTool")]
    private static void GenerateLevelFromToolbar()
    {
        ProcGenTool tool = FindObjectOfType<ProcGenTool>();
        if (tool != null)
        {
            tool.GenerateLevel();
        }
        else
        {
            Debug.LogWarning("No ProcGenTool found in the scene.");
        }
    }

    [MenuItem("Tools/Clear Level")]
    private static void ClearLevelFromToolbar()
    {
        ProcGenTool tool = FindObjectOfType<ProcGenTool>();
        if (tool != null)
        {
            tool.ClearLevel();
        }
        else
        {
            Debug.LogWarning("No ProcGenTool found in the scene.");
        }
    }
#endif
}