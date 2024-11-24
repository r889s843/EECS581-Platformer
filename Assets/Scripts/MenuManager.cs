using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_Dropdown modeDropdown;
    public TMP_Dropdown subMenuDropdown;
    public GameObject startButton;

    private int selectedDifficulty = 0;

    void Start()
    {
        modeDropdown.onValueChanged.AddListener(OnModeChanged); // add listener for selecting mode
        subMenuDropdown.AddOptions(new System.Collections.Generic.List<string> { "1", "2", "3", "4", "5", "6" });   // subDropdown starts with level select
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnStartGame); // initialize start button
    }

    void OnModeChanged(int index)
    {
        subMenuDropdown.ClearOptions(); // clear options
        subMenuDropdown.onValueChanged.RemoveAllListeners();    // clear listeners
        if (modeDropdown.options[index].text == "Levels") { //  if levels is selected
            subMenuDropdown.AddOptions(new System.Collections.Generic.List<string> { "1", "2", "3", "4", "5", "6" });   // add level options
        }
        else if (modeDropdown.options[index].text == "Procedural") {    // if procedural is selected
            subMenuDropdown.AddOptions(new System.Collections.Generic.List<string> { "Easy", "Medium", "Hard" });   // add difficulty options
            subMenuDropdown.onValueChanged.AddListener(OnDifficultyChanged);    // add difficulty listener
        }
    }

    void OnDifficultyChanged(int index)
    {
        selectedDifficulty = index; // get selected difficulty
    }

    void OnStartGame()
    {
        if (modeDropdown.options[modeDropdown.value].text == "Levels") {
            int levelIndex = subMenuDropdown.value + 1; // save selected level
            SceneManager.LoadScene(levelIndex); // load selected level
        }
        else if (modeDropdown.options[modeDropdown.value].text == "Procedural") {
            LevelManager.selectedDifficulty = selectedDifficulty; // pass selected difficulty to ProcGen.cs
            SceneManager.LoadScene(7); // load procedural level
        }
    }
}
