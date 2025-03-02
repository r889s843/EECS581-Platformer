using UnityEngine;
using UnityEngine.SceneManagement;

public class Start : MonoBehaviour
{
    public GameObject startButtonsPanel;
    public GameObject optionsButtonsPanel;

    public void StartStoryMode()
    {
        SceneManager.LoadScene(1);
    }

    public void StartFreeRun()
    {
        SceneManager.LoadScene(8);
    }

    public void OpenOptions()
    {
        startButtonsPanel.SetActive(false);
        optionsButtonsPanel.SetActive(true);
    }

    public void BackToStart()
    {
        optionsButtonsPanel.SetActive(false);
        startButtonsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
