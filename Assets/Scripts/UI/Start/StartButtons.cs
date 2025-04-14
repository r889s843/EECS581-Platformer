using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    public GameObject titleCard;
    public GameObject startButtonsPanel;
    public GameObject leaderboardsPanel;
    public GameObject optionsButtonsPanel;

    public void StartStoryMode()
    {
        SceneManager.LoadScene(1);
    }
    public void StartProcGen()
    {
        SceneManager.LoadScene(8);
    }

    public void StartFreeRun()
    {
        SceneManager.LoadScene(9);
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
        leaderboardsPanel.SetActive(false);
        optionsButtonsPanel.SetActive(false);
        startButtonsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
