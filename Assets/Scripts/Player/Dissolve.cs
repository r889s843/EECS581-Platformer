// Dissolve.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 3/2/2025
// Course: EECS 582
// Purpose: Manages the dissolve effect

using UnityEngine;

public class Dissolve: MonoBehaviour{
    [SerializeField] private Material material;

    private float dissolveAmount;
    private bool isDissolving;
    public float dissolveSpeed;

    private void Update()
    {
        if (isDissolving){
            dissolveAmount = Mathf.Clamp(dissolveAmount + dissolveSpeed * Time.deltaTime, 0, 1.1f);
            material.SetFloat("_DissolveAmount", dissolveAmount);
        } else {
            dissolveAmount = Mathf.Clamp(dissolveAmount - dissolveSpeed * Time.deltaTime, 0, 1.1f);
            material.SetFloat("_DissolveAmount", dissolveAmount);
        }
        if (Input.GetKeyDown(KeyCode.T)){
            isDissolving = true;
            Debug.Log("I should be dying");
        }
        if (Input.GetKeyDown(KeyCode.Y)){
            isDissolving = false;
            Debug.Log("I should be alive");
        }
    }

    public void StartDissolve(float dissolveSpeed){
        isDissolving = true;
        this.dissolveSpeed = dissolveSpeed;
    }
    public void StopDissolve(float dissolveSpeed){
        isDissolving = false;
        this.dissolveSpeed = dissolveSpeed;
    }
}