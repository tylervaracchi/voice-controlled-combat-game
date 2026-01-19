// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using UnityEngine;

/// <summary>
/// Centralized health management for both fighters.
/// Handles damage application, health bar UI updates, and death/round end conditions.
/// </summary>
public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float playerCurrentHealth;
    public float aiCurrentHealth;

    [Header("UI References")]
    public RectTransform playerHealthBar;
    public RectTransform aiHealthBar;

    [Header("Game Flow")]
    public PlayerHUDManager hudManager;

    void Start()
    {
        ResetHealth();
    }

    void Update()
    {
        UpdateHealthBars();
    }

    /// <summary>
    /// Update health bar UI to reflect current health values.
    /// Scales the health bar transform horizontally based on health percentage.
    /// </summary>
    private void UpdateHealthBars()
    {
        if (playerHealthBar != null)
        {
            float playerHealthScale = playerCurrentHealth / maxHealth;
            playerHealthBar.localScale = new Vector3(playerHealthScale, 1f, 1f);
        }

        if (aiHealthBar != null)
        {
            float aiHealthScale = aiCurrentHealth / maxHealth;
            aiHealthBar.localScale = new Vector3(aiHealthScale, 1f, 1f);
        }
    }

    /// <summary>
    /// Apply damage to specified character.
    /// </summary>
    /// <param name="characterTag">"Player" or "AI"</param>
    /// <param name="amount">Damage amount to apply</param>
    public void TakeDamage(string characterTag, float amount)
    {
        if (characterTag == "Player")
        {
            playerCurrentHealth -= amount;
            if (playerCurrentHealth <= 0)
            {
                playerCurrentHealth = 0;
                OnCharacterDeath("Player");
            }
        }
        else if (characterTag == "AI")
        {
            aiCurrentHealth -= amount;
            if (aiCurrentHealth <= 0)
            {
                aiCurrentHealth = 0;
                OnCharacterDeath("AI");
            }
        }
    }

    /// <summary>
    /// Handle character death and trigger round end.
    /// </summary>
    private void OnCharacterDeath(string characterTag)
    {
        if (hudManager != null)
        {
            if (characterTag == "Player")
            {
                hudManager.EndRound("Right"); // AI wins
            }
            else if (characterTag == "AI")
            {
                hudManager.EndRound("Left"); // Player wins
            }
        }
    }

    /// <summary>
    /// Reset both characters to full health (for new rounds).
    /// </summary>
    public void ResetHealth()
    {
        playerCurrentHealth = maxHealth;
        aiCurrentHealth = maxHealth;
        UpdateHealthBars();
    }

    /// <summary>
    /// Get player health as percentage (0-1).
    /// </summary>
    public float GetPlayerHealthPercent()
    {
        return playerCurrentHealth / maxHealth;
    }

    /// <summary>
    /// Get AI health as percentage (0-1).
    /// </summary>
    public float GetAIHealthPercent()
    {
        return aiCurrentHealth / maxHealth;
    }
}
