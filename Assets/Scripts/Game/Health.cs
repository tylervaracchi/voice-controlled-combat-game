// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using UnityEngine;

/// <summary>
/// Simple health component for combat characters.
/// Attach to both player and AI fighters.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

    public delegate void HealthChanged(float current, float max);
    public event HealthChanged OnHealthChanged;

    public delegate void CharacterDied();
    public event CharacterDied OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Apply damage to this character.
    /// </summary>
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal this character.
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Reset health to maximum (for round resets).
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}
