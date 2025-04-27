using UnityEngine;
using UnityEngine.UI;
using TMPro; // if you're using TextMeshPro

public class SaveSlot : MonoBehaviour
{
    public Image emptyFolder;
    public Image fullFolder;
    public Button createButton;
    public Button loadButton;
    public Button deleteButton;
    public TMP_InputField nameInputField;

    private void Start()
    {
        nameInputField.interactable = false;
        loadButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        nameInputField.onEndEdit.AddListener(OnEnterPressed);
    }

    public void OnCreateClicked()
    {
        nameInputField.interactable = true;
        nameInputField.ActivateInputField();
    }

    private void OnEnterPressed(string inputText)
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            nameInputField.interactable = false;
            emptyFolder.gameObject.SetActive(false);
            fullFolder.gameObject.SetActive(true);
            createButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);
        }
    }

    public void OnDeleteClicked()
    {
        emptyFolder.gameObject.SetActive(true);
        fullFolder.gameObject.SetActive(false);
        loadButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(false);
        createButton.gameObject.SetActive(true);
    }

    public void OnLoadClicked()
    {

    }
}
