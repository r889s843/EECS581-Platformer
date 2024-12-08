using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    // Player 1 icons
    public Image Lives1;
    public Image Lives2;
    public Image Lives3;

    // Player 2 icons
    public Image P2Lives1;
    public Image P2Lives2;
    public Image P2Lives3;

    public bool P2 = false;  // Determines if co-op mode is enabled

    public int currentLivesP1 = 3;
    public int currentLivesP2 = 3;

    void Start()
    {
        UpdateLivesDisplayP1();

        // If co-op isn't enabled, just hide P2's icons
        if (!P2) {
            P2Lives1.enabled = false;
            P2Lives2.enabled = false;
            P2Lives3.enabled = false;
        } else {
            UpdateLivesDisplayP2();
        }
    }

    public void LoseLifeP1()
    {
        currentLivesP1--;
        if (currentLivesP1 <= 0) {
            currentLivesP1 = 0;
            UpdateLivesDisplayP1();
            GoToMainScreen();
        } else {
            UpdateLivesDisplayP1();
        }
    }

    public void LoseLifeP2()
    {
        if (!P2) return; // If not co-op, no need to update P2 lives

        currentLivesP2--;
        if (currentLivesP2 <= 0) {
            currentLivesP2 = 0;
            UpdateLivesDisplayP2();
            GoToMainScreen();
        } else {
            UpdateLivesDisplayP2();
        }
    }

    private void UpdateLivesDisplayP1()
    {
        Lives1.enabled = (currentLivesP1 >= 1);
        Lives2.enabled = (currentLivesP1 >= 2);
        Lives3.enabled = (currentLivesP1 >= 3);
    }

    public void UpdateLivesDisplayP2()
    {
        // Only update if P2 is active
        if (P2) {
            P2Lives1.enabled = (currentLivesP2 >= 1);
            P2Lives2.enabled = (currentLivesP2 >= 2);
            P2Lives3.enabled = (currentLivesP2 >= 3);
        }
    }

    private void GoToMainScreen()
    {
        // Implement logic to return to the main menu or game over screen
        LevelManager.Instance.CheckLivesCondition(P2, currentLivesP1, currentLivesP2);
    }
}
