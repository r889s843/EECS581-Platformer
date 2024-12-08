using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_Dropdown modeDropdown;
    public TMP_Dropdown subMenuDropdown;
    public TMP_Dropdown coopDropdown;
    public GameObject startButton;
    public GameObject[] leaderboardTabs;

    private int coopTrigger = 0;
    private int selectedDifficulty = 1;

    void Start()
    {
        modeDropdown.onValueChanged.AddListener(OnModeChanged); // add listener for selecting mode
        coopDropdown.onValueChanged.AddListener(OnPlayerCountChanged); // add listener for selecting coop
        subMenuDropdown.AddOptions(new System.Collections.Generic.List<string> { "1", "2", "3", "4", "5", "6" });   // subDropdown starts with level select
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnStartGame); // initialize start button
    }

    void OnModeChanged(int index)
    {
        foreach (GameObject go in leaderboardTabs)
        {
            go.SetActive(false);
        }
        leaderboardTabs[index].SetActive(true);
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

    void OnPlayerCountChanged(int index)
    {
        coopTrigger = index;
    }

    void OnStartGame()
    {
        Debug.Log($"coopTrigger set to: {coopTrigger}");
        CoOpInitialize.coopTrigger = coopTrigger;
        if (modeDropdown.options[modeDropdown.value].text == "Levels") {
            CoOpInitialize.coopTrigger = coopTrigger;
            SceneManager.LoadScene(subMenuDropdown.value + 1); // load selected level
        }
        else if (modeDropdown.options[modeDropdown.value].text == "Procedural") {
            CoOpInitialize.coopTrigger = coopTrigger;
            LevelManager.selectedDifficulty = selectedDifficulty; // pass selected difficulty to ProcGen.cs
            SceneManager.LoadScene(7); // load procedural level
        }
        else if (modeDropdown.options[modeDropdown.value].text == "Freerun") {
            CoOpInitialize.coopTrigger = coopTrigger;
            SceneManager.LoadScene(8); // load freerun level
        }
    }
}
