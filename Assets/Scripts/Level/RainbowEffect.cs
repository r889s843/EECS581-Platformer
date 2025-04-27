using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapColorCycler : MonoBehaviour
{
    public float cycleSpeed = 1f; // Speed of the color cycle
    private Gradient colorGradient; // Gradient defining the color cycle

    private float currentTime = 0f;
    private Tilemap[] tilemaps;
    private Material[] tilemapMaterials; // Store materials for each tilemap

    void Start()
    {
        // Find and cache all Tilemap components in the scene
        tilemaps = FindObjectsOfType<Tilemap>();

        // Initialize materials array and get materials from TilemapRenderers
        tilemapMaterials = new Material[tilemaps.Length];
        for (int i = 0; i < tilemaps.Length; i++)
        {
            tilemapMaterials[i] = tilemaps[i].GetComponent<TilemapRenderer>().material;
            // Ensure the material is using the correct shader
            if (tilemapMaterials[i].shader.name != "Custom/TilemapColorReplace")
            {
                Debug.LogWarning($"Tilemap {tilemaps[i].name} is not using the correct shader. Please assign a material with the 'Custom/TilemapColorReplace' shader.");
            }
        }

        // Initialize the gradient if not set
        if (colorGradient == null)
        {
            colorGradient = CreateDefaultGradient();
        }
    }

    void Update()
    {
        // Increment time based on cycle speed
        currentTime += Time.deltaTime * cycleSpeed;
        if (currentTime > 1f) currentTime -= 1f; // Keep time between 0 and 1

        // Evaluate the gradient to get the current color
        Color currentColor = colorGradient.Evaluate(currentTime);

        // Apply the color to all tilemap materials
        foreach (Material material in tilemapMaterials)
        {
            material.SetColor("_ReplaceColor", currentColor);
        }
    }

    // Helper method to create a default neon rainbow gradient
    private Gradient CreateDefaultGradient()
    {
        Gradient gradient = new Gradient();

        // Define color keys for a bright neon rainbow: red, orange, yellow, green, blue, violet, red
        GradientColorKey[] colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.929f, 0f, 0.012f), 0f),       // Electric Red
            new GradientColorKey(new Color(1f, 0.525f, 0f), 0.167f),       // American Orange
            new GradientColorKey(new Color(1f, 0.996f, 0.216f), 0.333f),   // Electric Yellow
            new GradientColorKey(new Color(0.004f, 0.996f, 0.004f), 0.5f), // Electric Green
            new GradientColorKey(new Color(0.208f, 0f, 1f), 0.667f),       // Electric Ultramarine
            new GradientColorKey(new Color(0.549f, 0f, 0.988f), 0.833f),   // Electric Violet
            new GradientColorKey(new Color(0.929f, 0f, 0.012f), 1f)        // Back to Electric Red
        };

        // Define alpha keys (fully opaque)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }
}