using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    // Assign these in the Unity Inspector
    public GameObject[] portals = new GameObject[6]; // Array for the 6 portal GameObjects

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
    }

    private void Start()
    {
        UpdatePortals();
    }

    public void UpdatePortals()
    {
        // Validate the portals array
        if (portals.Length != 6)
        {
            Debug.LogError("PortalManager: Exactly 6 portals must be assigned in the Inspector.");
            return;
        }

        // Access the levelProgress array from PlayerData
        bool[] levelProgress = PlayerManager.Instance.playerData.levelProgress;
        if (levelProgress.Length != 6)
        {
            Debug.LogError("PortalManager: levelProgress array must have exactly 6 elements.");
            return;
        }

        // Check if all levels are completed
        bool allCompleted = true;
        for (int i = 0; i < 6; i++)
        {
            if (!levelProgress[i])
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            // All levels are completed, set all portals to inactive with completed color
            for (int i = 0; i < 6; i++)
            {
                SetPortalState(portals[i], false, completedColor);
            }
            return;
        }

        // Find the index of the next uncompleted level
        int nextLevelIndex = -1;
        for (int i = 0; i < 6; i++)
        {
            if (!levelProgress[i])
            {
                nextLevelIndex = i;
                break;
            }
        }

        // Update each portal's state and color based on its position relative to nextLevelIndex
        for (int i = 0; i < 6; i++)
        {
            if (i < nextLevelIndex)
            {
                // Completed portals (before the next uncompleted level)
                SetPortalState(portals[i], true, completedColor);
            }
            else if (i == nextLevelIndex)
            {
                // Next active portal
                SetPortalState(portals[i], true, activeColor);
            }
            else
            {
                // Locked portals (after the next uncompleted level)
                SetPortalState(portals[i], false, lockedColor);
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

        // Set the active state
        portal.SetActive(isActive);

        // Change the color (assuming a SpriteRenderer component)
        SpriteRenderer renderer = portal.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
        else
        {
            Debug.LogWarning("PortalManager: Portal does not have a SpriteRenderer component.");
        }
    }
}