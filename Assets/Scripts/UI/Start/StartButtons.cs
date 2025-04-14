using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    public GameObject startButtonsPanel;
    public GameObject optionsButtonsPanel;
    public GameObject leaderboardsPanel;

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
        startButtonsPanel.SetActive(false);
        leaderboardsPanel.SetActive(true);
    }
    
    public void OpenOptions()
    {
        startButtonsPanel.SetActive(false);
        optionsButtonsPanel.SetActive(true);
    }

    public void BackToStart()
    {
        optionsButtonsPanel.SetActive(false);
        leaderboardsPanel.SetActive(false);
        startButtonsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
