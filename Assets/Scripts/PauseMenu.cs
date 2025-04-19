using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Resume ()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause ()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        
        // Save player data using the PlayerManager instance
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SavePlayerData();
            Debug.Log("Player data saved successfully.");
        }
        else
        {
            Debug.LogError("PlayerManager instance is null. Cannot save player data.");
        }
        
        SceneManager.LoadScene(0); // Load the main menu scene
    }
}
