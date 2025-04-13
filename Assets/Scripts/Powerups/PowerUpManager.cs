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
    [Header("UI References")]
    public Image dashIcon;
    public Image teleportIcon;
    public Image invincibilityIcon;
    // public Image dashIcon;
    // public Image hookshotIcon;

    public TMPro.TextMeshProUGUI dashCooldownText;
    public TMPro.TextMeshProUGUI teleportCooldownText;
    public TMPro.TextMeshProUGUI invincibilityCooldownText;
    // public TMPro.TextMeshProUGUI dashCooldownText;
    // public TMPro.TextMeshProUGUI hookshotCooldownText;

    [Header("Cooldown Settings")]
    public float dashCooldownDuration = 3f;
    public float teleportCooldownDuration = 5f;
    public float invincibilityCooldownDuration = 5f;
    // public float aiStopCooldownDuration = 6f;
    // public float blinderCooldownDuration = 7f;

    private bool dashOnCooldown = false;
    private bool teleportOnCooldown = false;
    private bool invincibilityOnCooldown = false;
    // private bool aiStopOnCooldown = false;
    // private bool blinderOnCooldown = false;

    private float dashCooldownTimer;
    private float teleportCooldownTimer;
    private float invincibilityCooldownTimer;
    // private float aiStopCooldownTimer = 0f;
    // private float blinderCooldownTimer = 0f;
    
    public Dash_Powerup dashPowerupScript;
    public Teleport_Powerup teleportPowerupScript;
    public Invincible_Powerup invinciblePowerupScript;
    // public AIStop_Powerup aiStopPowerupScript;
    // public Blinder_Powerup blinderPowerupScript;


    // New booleans indicating if the ability is unlocked
    public bool invincibilityUnlocked = false;
    public bool dashUnlocked = false;
    public bool teleportUnlocked = false;
    // public bool aiStopUnlocked = false;
    // public bool blinderUnlocked = false;



        void Update()
    {
        // // Key 1: AI Stop Power-Up
        // if (Input.GetKeyDown(KeyCode.Alpha1) && aiStopUnlocked && !aiStopOnCooldown)
        // {
        //     aiStopPowerupScript.ActivatePowerup();
        //     StartCoroutine(CooldownRoutine(aiStopIcon, aiStopCooldownText, aiStopCooldownDuration, () => aiStopOnCooldown = false));
        //     aiStopOnCooldown = true;
        //     aiStopCooldownTimer = aiStopCooldownDuration;
        // }

        // Key 2: Invincibility Power-Up
        if (Input.GetKeyDown(KeyCode.R) && invincibilityUnlocked && !invincibilityOnCooldown)
        {
            invinciblePowerupScript.ActivatePowerup();
            StartCoroutine(CooldownRoutine(invincibilityIcon, invincibilityCooldownText, invincibilityCooldownDuration, () => invincibilityOnCooldown = false));
            invincibilityOnCooldown = true;
            invincibilityCooldownTimer = invincibilityCooldownDuration;
        }

        // // Key 3: Blinder Power-Up
        // if (Input.GetKeyDown(KeyCode.Alpha3) && blinderUnlocked && !blinderOnCooldown)
        // {
        //     blinderPowerupScript.ActivatePowerup();
        //     StartCoroutine(CooldownRoutine(blinderIcon, blinderCooldownText, blinderCooldownDuration, () => blinderOnCooldown = false));
        //     blinderOnCooldown = true;
        //     blinderCooldownTimer = blinderCooldownDuration;
        // }

        // Key 4 to Dash
        // if (Input.GetKeyDown(KeyCode.Alpha1)  && !dashOnCooldown)
        // {
        //     dashPowerupScript.ActivatePowerup();
        //     StartCoroutine(CooldownRoutine(dashIcon, dashCooldownText, dashCooldownDuration, () => dashOnCooldown = false));
        //     dashOnCooldown = true;
        //     dashCooldownTimer = dashCooldownDuration;
        // }

        // Key 5 to Teleport
        if (Input.GetKeyDown(KeyCode.Q) && !teleportOnCooldown)
        {
            teleportPowerupScript.ActivatePowerup();
            StartCoroutine(CooldownRoutine(teleportIcon, teleportCooldownText, teleportCooldownDuration, () => teleportOnCooldown = false));
            teleportOnCooldown = true;
            teleportCooldownTimer = teleportCooldownDuration;
        }

        // Update cooldown UI
        UpdateCooldownUI(ref invincibilityCooldownTimer, invincibilityCooldownText, ref invincibilityOnCooldown);
        // UpdateCooldownUI(ref aiStopCooldownTimer, aiStopCooldownText, ref aiStopOnCooldown);
        // UpdateCooldownUI(ref blinderCooldownTimer, blinderCooldownText, ref blinderOnCooldown);
        UpdateCooldownUI(ref dashCooldownTimer, dashCooldownText, ref dashOnCooldown);
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