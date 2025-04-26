using UnityEngine;

public class StarParallax : MonoBehaviour
{
    private GameObject cam;           // Reference to the camera
    public float parallaxEffect;      // Parallax effect multiplier (0 to 1)
    public float starFieldWidth;      // Width of the particle system star field (set in Inspector)

    private GameObject starField1;    // First star field (the original Stars GameObject)
    private GameObject starField2;    // Second star field (the duplicate for wrapping)
    private float startPosX;          // Starting X position of the star field
    private Vector3 initialPosition;  // Initial position of the star field for reference

    void Start()
    {
        // Find the camera (assuming the same hierarchy as before)
        cam = transform.parent.parent.gameObject;
        if (cam == null)
        {
            Debug.LogWarning("StarParallax: Camera not found. Ensure the Stars GameObject is a child of a parent under the Camera.");
            return;
        }

        // Store the initial position of this GameObject
        initialPosition = transform.position;
        startPosX = initialPosition.x;

        // The original star field is this GameObject
        starField1 = gameObject;

        // Create a duplicate star field for wrapping
        starField2 = Instantiate(gameObject, transform.parent);
        starField2.name = starField1.name + "_Duplicate";
        // Position the duplicate immediately to the right of the original
        starField2.transform.position = new Vector3(
            initialPosition.x + starFieldWidth,
            initialPosition.y,
            initialPosition.z
        );

        // Ensure the duplicate doesn’t have another StarParallax script to avoid recursion
        StarParallax duplicateParallax = starField2.GetComponent<StarParallax>();
        if (duplicateParallax != null)
        {
            Destroy(duplicateParallax);
        }
    }

    void Update()
    {
        if (cam == null) return;

        // Calculate the parallax offset based on camera movement
        float dist = cam.transform.position.x * parallaxEffect;

        // Update positions of both star fields with parallax effect
        starField1.transform.position = new Vector3(
            startPosX + dist,
            starField1.transform.position.y,
            starField1.transform.position.z
        );

        starField2.transform.position = new Vector3(
            startPosX + dist + starFieldWidth,
            starField2.transform.position.y,
            starField2.transform.position.z
        );

        // Calculate the camera's relative position to determine wrapping
        float cameraRelativeX = cam.transform.position.x * (1 - parallaxEffect);

        // Wrap the star fields when the camera nears the boundaries
        if (cameraRelativeX > startPosX + starFieldWidth)
        {
            // Camera has moved past the right edge of starField1
            startPosX += starFieldWidth;
        }
        else if (cameraRelativeX < startPosX - starFieldWidth)
        {
            // Camera has moved past the left edge of starField1
            startPosX -= starFieldWidth;
        }
    }

    void OnDestroy()
    {
        // Clean up the duplicate star field when this GameObject is destroyed
        if (starField2 != null)
        {
            Destroy(starField2);
        }
    }
}