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
        subMenuDropdown.gameObject.SetActive(true);
        subMenuDropdown.ClearOptions(); // clear options
        subMenuDropdown.onValueChanged.RemoveAllListeners();    // clear listeners
        if (modeDropdown.options[index].text == "Levels") { //  if levels is selected
            subMenuDropdown.AddOptions(new System.Collections.Generic.List<string> { "1", "2", "3", "4", "5", "6" });   // add level options
        }
        else if (modeDropdown.options[index].text == "Procedural") {    // if procedural is selected
            subMenuDropdown.AddOptions(new System.Collections.Generic.List<string> { "Easy", "Medium", "Hard" });   // add difficulty options
            subMenuDropdown.onValueChanged.AddListener(OnDifficultyChanged);    // add difficulty listener
        }
        else if (modeDropdown.options[index].text == "Freerun") {
            subMenuDropdown.gameObject.SetActive(false);
        }
    }

    void OnDifficultyChanged(int index)
    {
        selectedDifficulty = index; // get selected difficulty
    }

    void OnStartGame()
    {
        if (modeDropdown.options[modeDropdown.value].text == "Levels") {
            SceneManager.LoadScene(subMenuDropdown.value + 1); // load selected level
        }
        else if (modeDropdown.options[modeDropdown.value].text == "Procedural") {
            LevelManager.selectedDifficulty = selectedDifficulty; // pass selected difficulty to ProcGen.cs
            SceneManager.LoadScene(7); // load procedural level
        }
        else if (modeDropdown.options[modeDropdown.value].text == "Freerun") {
            SceneManager.LoadScene(8); // load freerun level
        }
    }
}
