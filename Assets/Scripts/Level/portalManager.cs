using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    // Assign these in the Unity Inspector for the 6 main portals
    public GameObject[] portals = new GameObject[6]; // Array for the 6 main portal GameObjects

    // References for the Free Run and Proc Gen portals (found as children)
    private GameObject freeRunPortal;
    private GameObject procGenPortal;

    // Colors for different portal states
    public Color completedColor = Color.gray;  // Color for completed portals
    public Color activeColor = Color.green;    // Color for the next active portal
    public Color lockedColor = Color.red;      // Color for locked (future) portals

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes

        // Find the Free Run and Proc Gen portals in children
        FindAdditionalPortals();
    }

    private void Start()
    {
        UpdatePortals();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find the portals in case the hierarchy changes
        FindAdditionalPortals();
        UpdatePortals();
    }

    private void FindAdditionalPortals()
    {
        freeRunPortal = null;
        procGenPortal = null;

        // Find Free Run and Proc Gen portals in children by name
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "FreeRunPortal")
            {
                freeRunPortal = child.gameObject;
                Debug.Log("PortalManager: Found FreeRunPortal in children.");
            }
            else if (child.gameObject.name == "ProcGenPortal")
            {
                procGenPortal = child.gameObject;
                Debug.Log("PortalManager: Found ProcGenPortal in children.");
            }
        }

        if (freeRunPortal == null)
            Debug.LogWarning("PortalManager: FreeRunPortal not found in children. Ensure it is named 'FreeRunPortal' and is a child of PortalManager.");
        if (procGenPortal == null)
            Debug.LogWarning("PortalManager: ProcGenPortal not found in children. Ensure it is named 'ProcGenPortal' and is a child of PortalManager.");
    }

    public void UpdatePortals()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"PortalManager: Updating portals in scene '{sceneName}' (index {currentSceneIndex})");

        // If the current scene is not "Shop" (index 1), disable all portals
        if (currentSceneIndex != 1)
        {
            Debug.Log("PortalManager: Not in Shop scene, disabling all portals.");
            // Disable main portals
            for (int i = 0; i < portals.Length; i++)
            {
                SetPortalState(portals[i], false, lockedColor);
            }
            // Disable Free Run and Proc Gen portals
            SetPortalState(freeRunPortal, false, Color.white);
            SetPortalState(procGenPortal, false, Color.white);
            return;
        }

        // === Shop Scene Logic ===
        Debug.Log("PortalManager: In Shop scene, updating portals based on PlayerData.");

        // Enable Free Run and Proc Gen portals in Shop scene (no color change, keep default state)
        SetPortalState(freeRunPortal, true, Color.white);
        Debug.Log("PortalManager: FreeRunPortal enabled in Shop scene with default state.");
        SetPortalState(procGenPortal, true, Color.white);
        Debug.Log("PortalManager: ProcGenPortal enabled in Shop scene with default state.");

        // Validate the main portals array
        if (portals.Length != 6)
        {
            Debug.LogError("PortalManager: Exactly 6 main portals must be assigned in the Inspector.");
            return;
        }

        // Access the levelProgress array from PlayerData
        bool[] levelProgress = PlayerManager.Instance.playerData.levelProgress;
        if (levelProgress.Length != 6)
        {
            Debug.LogError("PortalManager: levelProgress array must have exactly 6 elements.");
            return;
        }

        // Check if all main levels are completed
        bool allMainLevelsCompleted = true;
        for (int i = 0; i < 6; i++)
        {
            if (!levelProgress[i])
            {
                allMainLevelsCompleted = false;
                break;
            }
        }

        // Update main portals
        if (allMainLevelsCompleted)
        {
            Debug.Log("PortalManager: All main levels completed, setting main portals to completed state.");
            for (int i = 0; i < 6; i++)
            {
                SetPortalState(portals[i], false, completedColor);
            }
        }
        else
        {
            int nextLevelIndex = -1;
            for (int i = 0; i < 6; i++)
            {
                if (!levelProgress[i])
                {
                    nextLevelIndex = i;
                    break;
                }
            }

            Debug.Log($"PortalManager: Next uncompleted level is {nextLevelIndex}.");
            for (int i = 0; i < 6; i++)
            {
                if (i < nextLevelIndex)
                {
                    SetPortalState(portals[i], true, completedColor);
                }
                else if (i == nextLevelIndex)
                {
                    SetPortalState(portals[i], true, activeColor);
                }
                else
                {
                    SetPortalState(portals[i], false, lockedColor);
                }
            }
        }
    }

    private void SetPortalState(GameObject portal, bool isActive, Color color)
    {
        if (portal == null)
        {
            Debug.LogWarning("PortalManager: A portal reference is null.");
            return;
        }

        portal.SetActive(isActive);

        // Only change the color if a specific color is provided (not for Free Run/Proc Gen in Shop)
        SpriteRenderer renderer = portal.GetComponent<SpriteRenderer>();
        if (renderer != null && color != Color.white) // Color.white is used as a placeholder to skip color change
        {
            renderer.color = color;
        }
        else if (renderer == null)
        {
            Debug.LogWarning("PortalManager: Portal does not have a SpriteRenderer component.");
        }
    }
}