// NPC.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/12/2025
// Course: EECS 582
// Purpose: Adds NPC dialogue options to the game

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject dialoguePanel;
    public TMPro.TextMeshProUGUI dialogueText;
    public string[] dialogue;
    private int index;

    public GameObject continueButton;
    public float wordSpeed = 0.06f; // default is 0.06
    public bool playerIsClose;

    public GameObject shopPanel;

    void Start()
    {
        dialogueText.text = "";
        shopPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && playerIsClose){
            if(dialoguePanel.activeInHierarchy){
                zeroText();
            }
            else{
                dialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
        }
        if(dialogueText.text == dialogue[index]){
            continueButton.SetActive(true);
        }
    }

    public void zeroText(){
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);
    }

    IEnumerator Typing(){
        foreach(char letter in dialogue[index].ToCharArray()){
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }


    public void NextLine(){
        continueButton.SetActive(false);

        if (index < dialogue.Length -1){
            index++;
            dialogueText.text = "";
            StartCoroutine(Typing());
        } else {
            // Dialogue has ended
            zeroText(); // Close the dialogue panel

            // Activate the shop panel
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")){
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")){
            playerIsClose = false;
            zeroText();
        }
    }
}
