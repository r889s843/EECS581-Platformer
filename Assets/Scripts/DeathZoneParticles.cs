using UnityEngine;

public class DeathZoneParticles : MonoBehaviour
{
    public ParticleSystem particleSystem; // Reference to the particle system

    void Start()
    {
        AdjustParticleSystemSize();
    }

    void AdjustParticleSystemSize()
    {
        if (particleSystem == null) return;

        // Get the width of the deathZone from its collider
        float width = GetComponent<BoxCollider2D>().size.x * transform.localScale.x;

        // Modify the shape module to match the width
        var shape = particleSystem.shape;
        shape.scale = new Vector3(width, shape.scale.y, shape.scale.z);
    }
}