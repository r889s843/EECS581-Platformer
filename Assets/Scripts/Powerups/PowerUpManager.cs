// PowerUpManager.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/15/2025
// Course: EECS 582
// Purpose: Controls the activation of player inventory powerup/abilities

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool isPlayer2 = false; // Inspector-assigned boolean to determine if this is Player 2

    [Header("UI References")]
    public Image teleportIcon;
    public Image invincibilityIcon;

    public TMPro.TextMeshProUGUI teleportCooldownText;
    public TMPro.TextMeshProUGUI invincibilityCooldownText;

    [Header("Cooldown Settings")]
    public float teleportCooldownDuration = 5f;
    public float invincibilityCooldownDuration = 5f;

    private bool teleportOnCooldown = false;
    private bool invincibilityOnCooldown = false;

    private float teleportCooldownTimer;
    private float invincibilityCooldownTimer;
    
    public Teleport_Powerup teleportPowerupScript;
    public Invincible_Powerup invinciblePowerupScript;

    // New booleans indicating if the ability is unlocked
    public bool invincibilityUnlocked = false;
    public bool teleportUnlocked = false;

    void Update()
    {
        if (!isPlayer2) // Player 1 key bindings
        {
            // Key R: Invincibility Power-Up (Player 1)
            if (Input.GetKeyDown(KeyCode.R) && invincibilityUnlocked && !invincibilityOnCooldown)
            {
                invinciblePowerupScript.ActivatePowerup();
                StartCoroutine(CooldownRoutine(invincibilityIcon, invincibilityCooldownText, invincibilityCooldownDuration, () => invincibilityOnCooldown = false));
                invincibilityOnCooldown = true;
                invincibilityCooldownTimer = invincibilityCooldownDuration;
            }

            // Key Q: Teleport (Player 1)
            if (Input.GetKeyDown(KeyCode.Q) && teleportUnlocked && !teleportOnCooldown)
            {
                teleportPowerupScript.ActivatePowerup();
                StartCoroutine(CooldownRoutine(teleportIcon, teleportCooldownText, teleportCooldownDuration, () => teleportOnCooldown = false));
                teleportOnCooldown = true;
                teleportCooldownTimer = teleportCooldownDuration;
            }
        }
        else // Player 2 key bindings
        {
            // Key T: Invincibility Power-Up (Player 2)
            if ((Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.RightAlt)) && invincibilityUnlocked && !invincibilityOnCooldown)
            {
                invinciblePowerupScript.ActivatePowerup();
                StartCoroutine(CooldownRoutine(invincibilityIcon, invincibilityCooldownText, invincibilityCooldownDuration, () => invincibilityOnCooldown = false));
                invincibilityOnCooldown = true;
                invincibilityCooldownTimer = invincibilityCooldownDuration;
            }

            // Key U: Teleport (Player 2)
            if ((Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.RightShift)) && teleportUnlocked && !teleportOnCooldown)
            {
                teleportPowerupScript.ActivatePowerup();
                StartCoroutine(CooldownRoutine(teleportIcon, teleportCooldownText, teleportCooldownDuration, () => teleportOnCooldown = false));
                teleportOnCooldown = true;
                teleportCooldownTimer = teleportCooldownDuration;
            }
        }

        // Update cooldown UI
        UpdateCooldownUI(ref invincibilityCooldownTimer, invincibilityCooldownText, ref invincibilityOnCooldown);
        UpdateCooldownUI(ref teleportCooldownTimer, teleportCooldownText, ref teleportOnCooldown);
    }

    private IEnumerator CooldownRoutine(Image icon, TMPro.TextMeshProUGUI cooldownText, float duration, System.Action onCooldownEnd)
    {
        icon.color = new Color(0.5f, 0.5f, 0.5f);
        float timer = duration;

        while (timer > 0)
        {
            cooldownText.text = Mathf.Ceil(timer).ToString();
            timer -= Time.deltaTime;
            yield return null;
        }

        cooldownText.text = "";
        icon.color = Color.white;
        onCooldownEnd?.Invoke();
    }

    private void UpdateCooldownUI(ref float timer, TMPro.TextMeshProUGUI cooldownText, ref bool isOnCooldown)
    {
        if (isOnCooldown)
        {
            timer -= Time.deltaTime;
            cooldownText.text = Mathf.Ceil(timer).ToString();

            if (timer <= 0f)
            {
                cooldownText.text = "";
                isOnCooldown = false;
            }
        }
    }
}