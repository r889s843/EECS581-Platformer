using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveSlot : MonoBehaviour
{
    public Image emptyFolder;
    public Image fullFolder;
    public Button createButton;
    public Button loadButton;
    public Button deleteButton;
    public TMP_InputField nameInputField;
    public int slotIndex; // Set in Inspector (e.g., 1, 2, 3)

    private void Start()
    {
        UpdateSlotUI();
        nameInputField.onEndEdit.AddListener(OnEnterPressed);
    }

    private void UpdateSlotUI()
    {
        SaveData saveData = LoadSystem.LoadGameData(slotIndex);
        if (saveData != null)
        {
            emptyFolder.gameObject.SetActive(false);
            fullFolder.gameObject.SetActive(true);
            createButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);
            nameInputField.text = saveData.playerData.username;
            nameInputField.interactable = false;
        }
        else
        {
            emptyFolder.gameObject.SetActive(true);
            fullFolder.gameObject.SetActive(false);
            createButton.gameObject.SetActive(true);
            loadButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            nameInputField.text = "";
            nameInputField.interactable = false;
        }
    }

    public void OnCreateClicked()
    {
        nameInputField.interactable = true;
        nameInputField.ActivateInputField();
    }

    private void OnEnterPressed(string inputText)
    {
        if (!string.IsNullOrEmpty(inputText))
        {
            PlayerData newPlayerData = new PlayerData
            {
                username = inputText,
                money = 0f,
                levelProgress = new bool[6],
                abilitiesUnlocked = new bool[4],
                bestFreerunDistance = 0f,
                procGenCompletionCount = 0,
                bestStoryTime = 0f
            };
            PlayerManager.Instance.playerData = newPlayerData;
            PlayerManager.Instance.currentSlotIndex = slotIndex;
            PlayerManager.Instance.SavePlayerData();
            nameInputField.interactable = false;
            UpdateSlotUI();
            SceneManager.LoadScene(1); // Load "Shop" scene
        }
    }

    public void OnDeleteClicked()
    {
        SaveSystem.DeleteSaveData(slotIndex);
        UpdateSlotUI();
    }

    public void OnLoadClicked()
    {
        SaveData saveData = LoadSystem.LoadGameData(slotIndex);
        if (saveData != null)
        {
            PlayerManager.Instance.playerData = saveData.playerData;
            PlayerManager.Instance.currentSlotIndex = slotIndex;
            SceneManager.LoadScene(1); // Load "Shop" scene
        }
    }
}