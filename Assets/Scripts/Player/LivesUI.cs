// LivesUI.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/05/2024
// Course: EECS 581
// Purpose: Manage lives of players

using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    // Player 1 life icons
    public Image Lives1; // First life icon for Player 1
    public Image Lives2; // Second life icon for Player 1
    public Image Lives3; // Third life icon for Player 1

    // Player 2 life icons
    public Image P2Lives1; // First life icon for Player 2
    public Image P2Lives2; // Second life icon for Player 2
    public Image P2Lives3; // Third life icon for Player 2

    public bool P2 = false;  // Determines if co-op mode is enabled

    public int currentLivesP1 = 3; // Current lives for Player 1
    public int currentLivesP2 = 3; // Current lives for Player 2

    public bool isUnlimitedLivesScene = false; // Flag for unlimited lives in Level5
    private Color greyedOutColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Color for greyed-out icons

    void Start()
    {
        UpdateLivesDisplayP1(); // Initialize Player 1 lives display

        // If co-op isn't enabled, hide Player 2's life icons
        if (!P2) {
            P2Lives1.enabled = false;
            P2Lives2.enabled = false;
            P2Lives3.enabled = false;
        } else {
            UpdateLivesDisplayP2(); // Initialize Player 2 lives display
        }
        if (isUnlimitedLivesScene){
            GreyOutLivesP1();
            if (P2){
                GreyOutLivesP2();
            }
        }
    }

    public void LoseLifeP1()
    {
        if (!isUnlimitedLivesScene){
            currentLivesP1--; // Decrement Player 1's lives
            if (currentLivesP1 <= 0) {
                currentLivesP1 = 0; // Ensure lives don't go below zero
                UpdateLivesDisplayP1(); // Update display
                GoToMainScreen(); // Handle game over or main menu transition
            } else {
                UpdateLivesDisplayP1(); // Update display
            }
        }
    }

    public void LoseLifeP2()
    {
        if (!P2) return; // If not co-op, no need to update Player 2 lives

        if (!isUnlimitedLivesScene){
            currentLivesP2--; // Decrement Player 2's lives
            if (currentLivesP2 <= 0) {
                currentLivesP2 = 0; // Ensure lives don't go below zero
                UpdateLivesDisplayP2(); // Update display
                GoToMainScreen(); // Handle game over or main menu transition
            } else {
                UpdateLivesDisplayP2(); // Update display
            }
        }
    }

    private void UpdateLivesDisplayP1()
    {
        Lives1.enabled = (currentLivesP1 >= 1); // Show first life icon if Player 1 has at least 1 life
        Lives2.enabled = (currentLivesP1 >= 2); // Show second life icon if Player 1 has at least 2 lives
        Lives3.enabled = (currentLivesP1 >= 3); // Show third life icon if Player 1 has 3 lives
    }

    public void UpdateLivesDisplayP2()
    {
        // Only update if P2 is active
        if (P2) {
            P2Lives1.enabled = (currentLivesP2 >= 1); // Show first life icon for Player 2 if at least 1 life
            P2Lives2.enabled = (currentLivesP2 >= 2); // Show second life icon for Player 2 if at least 2 lives
            P2Lives3.enabled = (currentLivesP2 >= 3); // Show third life icon for Player 2 if 3 lives
        }
    }

    private void GreyOutLivesP1()
    {
        Lives1.enabled = true; // Ensure icon is visible
        Lives1.color = greyedOutColor; // Apply greyed-out color
        Lives2.enabled = true;
        Lives2.color = greyedOutColor;
        Lives3.enabled = true;
        Lives3.color = greyedOutColor;
    }

    private void GreyOutLivesP2()
    {
        Lives1.enabled = true; // Ensure icon is visible
        Lives1.color = greyedOutColor; // Apply greyed-out color
        Lives2.enabled = true;
        Lives2.color = greyedOutColor;
        Lives3.enabled = true;
        Lives3.color = greyedOutColor;
    }

    private void GoToMainScreen()
    {
        // logic to return to the main menu or game over screen
        LevelManager.Instance.CheckLivesCondition(P2, currentLivesP1, currentLivesP2); // Notify LevelManager about players' lives
    }
}
