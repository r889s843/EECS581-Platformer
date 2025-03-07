// Dissolve.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 3/2/2025
// Course: EECS 582
// Purpose: Manages the dissolve effect

using UnityEngine;
using System.Collections;

public class Dissolve: MonoBehaviour{
    [SerializeField] public Material material;

    private float dissolveAmount;
    public bool isDissolving;
    public float dissolveSpeed;

    // private void Start()
    // {
    //     material = new Material(material);
    //     GetComponent<Renderer>().material = material;
    //     material.SetFloat("_DissolveAmount", 0f);
    // }

    public void StartDissolve(float dissolveSpeed)
    {
        isDissolving = true;
        this.dissolveSpeed = dissolveSpeed;
        StartCoroutine(DissolveRoutine(true));
    }

    public void StopDissolve(float dissolveSpeed)
    {
        isDissolving = false;
        this.dissolveSpeed = dissolveSpeed;
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