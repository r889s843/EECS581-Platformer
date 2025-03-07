// Dissolve.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 3/2/2025
// Course: EECS 582
// Purpose: Manages the dissolve effect

using UnityEngine;
using System.Collections;

public class Dissolve: MonoBehaviour{
    [SerializeField] public Material material;
    [SerializeField] private ParticleSystem OneParticles; // Particle System Reference
    [SerializeField] private ParticleSystem ZeroParticles; // Particle System Reference

    [SerializeField] private ParticleSystem OneParticlesInverse; // Particle System Reference
    [SerializeField] private ParticleSystem ZeroParticlesInverse; // Particle System Reference

    private float dissolveAmount;
    public bool isDissolving;
    public float dissolveSpeed;


    public void StartDissolve(float dissolveSpeed)
    {
        isDissolving = true;
        this.dissolveSpeed = dissolveSpeed;

        OneParticles.transform.position = transform.position;
        // Scale the particle effect
        OneParticles.transform.localScale = transform.localScale;
        var mainModule = OneParticles.main;
        mainModule.startSizeMultiplier = transform.localScale.x; // Scale particles with object
        var shapeModule = OneParticles.shape;
        shapeModule.scale = transform.localScale; // Scale shape of particles
        OneParticles.Play();


        ZeroParticles.transform.position = transform.position;
        // Scale the particle effect
        ZeroParticles.transform.localScale = transform.localScale;
        var mainModule2 = ZeroParticles.main;
        mainModule2.startSizeMultiplier = transform.localScale.x; // Scale particles with object
        var shapeModule2 = ZeroParticles.shape;
        shapeModule2.scale = transform.localScale; // Scale shape of particles
        ZeroParticles.Play();


        StartCoroutine(DissolveRoutine(true));
    }

    public void StopDissolve(float dissolveSpeed)
    {
        isDissolving = false;
        this.dissolveSpeed = dissolveSpeed;

        Vector3 newPosition = ZeroParticlesInverse.transform.position;
        newPosition.x = transform.position.x;
        newPosition.y = transform.position.y+2f;

        OneParticlesInverse.transform.position = newPosition;
        // Scale the particle effect
        OneParticlesInverse.transform.localScale = transform.localScale;
        var mainModule3 = OneParticlesInverse.main;
        mainModule3.startSizeMultiplier = transform.localScale.x; // Scale particles with object
        var shapeModule3 = OneParticlesInverse.shape;
        shapeModule3.scale = transform.localScale; // Scale shape of particles
        OneParticlesInverse.Play();


        ZeroParticlesInverse.transform.position = newPosition;
        // Scale the particle effect
        ZeroParticlesInverse.transform.localScale = transform.localScale;
        var mainModule4 = ZeroParticlesInverse.main;
        mainModule4.startSizeMultiplier = transform.localScale.x; // Scale particles with object
        var shapeModule4 = ZeroParticlesInverse.shape;
        shapeModule4.scale = transform.localScale; // Scale shape of particles
        ZeroParticlesInverse.Play();


        StartCoroutine(DissolveRoutine(false));
    }

    private IEnumerator DissolveRoutine(bool dissolving)
    {
        while ((dissolving && dissolveAmount < 1.1f) || (!dissolving && dissolveAmount > 0f))
        {
            dissolveAmount = Mathf.Clamp(
                dissolveAmount + (dissolving ? dissolveSpeed : -dissolveSpeed) * Time.deltaTime,
                0, 1.1f
            );

            material.SetFloat("_DissolveAmount", dissolveAmount);
            yield return null; // Wait for next frame
        }

        // Debug.Log(dissolving ? "Dissolve Complete" : "Restore Complete");
    }

}