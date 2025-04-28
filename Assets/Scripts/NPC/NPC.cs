// NPC.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/12/2025
// Course: EECS 582
// Purpose: Adds NPC dialogue options to the game

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    // Dialogue UI elements
    public GameObject dialoguePanel;
    public TMPro.TextMeshProUGUI dialogueText;
    public GameObject continueButton;
    public float wordSpeed = 0.01f; // Speed of text typing

    // NPC configuration
    [System.Serializable]
    public class NPCConfig
    {
        public string npcName;           // Name of the NPC (e.g., "Zip")
        public string level;             // Level this config applies to (e.g., "Level5")
        public string[] dialogue;        // Dialogue lines for this NPC in this level
        public bool hasShop;             // Whether this NPC has a shop in this level
        public bool saveGameOnDialogueEnd; // Whether to save the game when dialogue ends
        public bool unlockFeatureOnDialogueEnd; // Whether to unlock a feature when dialogue ends
        public string featureToUnlock;   // Name of the feature to unlock (e.g., "DoubleJump")
        public bool endConversationOnLastLine; // Whether to just end the conversation without further action
    }

    public List<NPCConfig> npcConfigs = new List<NPCConfig>(); // List of configurations for this NPC
    private NPCConfig currentConfig; // The current configuration being used
    private int dialogueIndex; // Current index in the dialogue array

    // Shop UI
    public GameObject shopPanel;

    // Player interaction state
    private bool playerIsClose;
    private bool isTyping; // Flag to track if typing is in progress

    void Start()
    {
        // Dynamically find continueButton if not assigned or invalid
        if (dialoguePanel != null && (continueButton == null || !continueButton.transform.IsChildOf(dialoguePanel.transform)))
        {
            continueButton = dialoguePanel.transform.Find("ContinueButton")?.gameObject;
            if (continueButton == null)
            {
                Debug.LogError($"NPC {gameObject.name}: Could not find ContinueButton under {dialoguePanel.name}.");
            }
            else
            {
                Debug.Log($"NPC {gameObject.name}: Assigned continueButton to {continueButton.name}");
            }
        }

        // Validate UI references
        if (dialoguePanel == null || dialogueText == null || continueButton == null)
        {
            Debug.LogError($"NPC {gameObject.name}: Missing UI references. Ensure dialoguePanel, dialogueText, and continueButton are assigned or found. " +
                           $"dialoguePanel={(dialoguePanel != null ? dialoguePanel.name : "null")}, " +
                           $"dialogueText={(dialogueText != null ? dialogueText.name : "null")}, " +
                           $"continueButton={(continueButton != null ? continueButton.name : "null")}");
            return;
        }

        dialogueText.text = "";
        continueButton.SetActive(false);
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Determine the current level and select the appropriate config
        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        SelectConfig(currentLevel);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if (dialoguePanel.activeInHierarchy)
            {
                if (isTyping)
                {
                    // If typing, finish the current line immediately
                    StopCoroutine(Typing());
                    dialogueText.text = currentConfig.dialogue[dialogueIndex];
                    isTyping = false;
                    if (continueButton != null)
                    {
                        continueButton.SetActive(true);
                    }
                }
                else
                {
                    // If not typing, close the dialogue
                    ZeroText();
                }
            }
            else
            {
                dialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
        }

        // Only show the continue button when typing is complete
        if (!isTyping && dialogueText.text == currentConfig.dialogue[dialogueIndex])
        {
            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }
        }
    }

    private void SelectConfig(string level)
    {
        // Find the config that matches the current level and NPC
        foreach (var config in npcConfigs)
        {
            if (config.level == level)
            {
                currentConfig = config;
                dialogueIndex = 0;
                return;
            }
        }

        // Fallback: Use the first config if no match is found
        if (npcConfigs.Count > 0)
        {
            currentConfig = npcConfigs[0];
            Debug.LogWarning($"NPC: No config found for level {level}. Using default config for {currentConfig.npcName}.");
        }
        else
        {
            Debug.LogError("NPC: No configurations assigned in npcConfigs.");
            currentConfig = new NPCConfig { dialogue = new string[] { "Error: No dialogue configured." } };
        }
    }

    public void ZeroText()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
        dialogueIndex = 0;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        isTyping = false;
    }

    IEnumerator Typing()
    {
        isTyping = true;
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        dialogueText.text = "";
        foreach (char letter in currentConfig.dialogue[dialogueIndex].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
        isTyping = false;
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void NextLine()
    {
        if (isTyping)
        {
            // If still typing, finish the current line immediately
            StopCoroutine(Typing());
            dialogueText.text = currentConfig.dialogue[dialogueIndex];
            isTyping = false;
            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"NPC {gameObject.name}: continueButton is null in NextLine() during typing completion.");
            }
            return;
        }

        // Null check for continueButton before accessing it
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"NPC {gameObject.name}: continueButton is null in NextLine().");
        }

        if (dialogueIndex < currentConfig.dialogue.Length - 1)
        {
            // More dialogue lines to show
            dialogueIndex++;
            StartCoroutine(Typing());
        }
        else
        {
            // Dialogue has ended, determine the next action based on config
            HandleDialogueEnd();
        }
    }

    private void HandleDialogueEnd()
    {
        // First, close the dialogue panel
        ZeroText();

        // Perform actions based on the current config
        if (currentConfig.endConversationOnLastLine)
        {
            // Just end the conversation, no further action
            return;
        }

        // Open shop if the NPC has one
        if (currentConfig.hasShop && shopPanel != null)
        {
            shopPanel.SetActive(true);
            return;
        }

        // Save the game if configured
        if (currentConfig.saveGameOnDialogueEnd)
        {
            PlayerManager.Instance.SavePlayerData();
            Debug.Log($"NPC: Game saved after dialogue with {currentConfig.npcName} in {currentConfig.level}.");
        }

        // Unlock a feature if configured
        if (currentConfig.unlockFeatureOnDialogueEnd && !string.IsNullOrEmpty(currentConfig.featureToUnlock))
        {
            UnlockFeature(currentConfig.featureToUnlock);
        }
    }

    private void UnlockFeature(string feature)
    {
        // Example: Unlock abilities in PlayerData based on the feature name
        switch (feature.ToLower())
        {
            case "doublejump":
                PlayerManager.Instance.playerData.abilitiesCanBePurchased[0] = true;
                Debug.Log("NPC: Unlocked Double Jump ability.");
                break;
            case "dash":
                PlayerManager.Instance.playerData.abilitiesCanBePurchased[1] = true;
                Debug.Log("NPC: Unlocked Dash ability.");
                break;
            case "teleport":
                PlayerManager.Instance.playerData.abilitiesCanBePurchased[2] = true;
                Debug.Log("NPC: Unlocked Teleport ability.");
                break;
            case "invincibility":
                PlayerManager.Instance.playerData.abilitiesCanBePurchased[3] = true;
                Debug.Log("NPC: Unlocked Invincibility ability.");
                break;
            case "aistop":
                PlayerManager.Instance.playerData.abilitiesCanBePurchased[4] = true;
                Debug.Log("NPC: Unlocked AI stop ability.");
                break;
            default:
                Debug.LogWarning($"NPC: Unknown feature to unlock: {feature}");
                break;
        }
        PlayerManager.Instance.SavePlayerData(); // Save after unlocking
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = false;
            ZeroText();
            CloseShop();
        }
    }

    void OnEnable()
    {
        if (continueButton != null)
        {
            UnityEngine.UI.Button button = continueButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(NextLine);
            }
        }
    }
}