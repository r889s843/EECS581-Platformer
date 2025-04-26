using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    public GameObject titleCard;
    public GameObject startButtonsPanel;
    public GameObject savePanel;
    public GameObject leaderboardsPanel;
    public GameObject optionsButtonsPanel;

    public void OpenSaves()
    {
        titleCard.SetActive(false);
        startButtonsPanel.SetActive(false);
        savePanel.SetActive(true);
    }

    public void OpenLeaderboards()
    {
        titleCard.SetActive(false);
        startButtonsPanel.SetActive(false);
        leaderboardsPanel.SetActive(true);
    }
    
    public void OpenOptions()
    {
        titleCard.SetActive(false);
        startButtonsPanel.SetActive(false);
        optionsButtonsPanel.SetActive(true);
    }

    public void BackToStart()
    {
        titleCard.SetActive(true);
        savePanel.SetActive(false);
        leaderboardsPanel.SetActive(false);
        optionsButtonsPanel.SetActive(false);
        startButtonsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
