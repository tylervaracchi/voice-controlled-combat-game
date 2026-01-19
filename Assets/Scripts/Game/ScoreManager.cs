// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using UnityEngine;

/// <summary>
/// Static score manager for tracking wins/losses across sessions.
/// Uses PlayerPrefs for persistent storage.
/// </summary>
public static class ScoreManager
{
    private static readonly string WinsKey = "Wins";
    private static readonly string LossesKey = "Losses";

    // Session-only tracking
    private static int playerWins;
    private static int aiWins;

    #region Persistent Stats (PlayerPrefs)

    /// <summary>
    /// Increment total wins (persistent).
    /// </summary>
    public static void IncrementWins()
    {
        int currentWins = PlayerPrefs.GetInt(WinsKey, 0);
        PlayerPrefs.SetInt(WinsKey, currentWins + 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Increment total losses (persistent).
    /// </summary>
    public static void IncrementLosses()
    {
        int currentLosses = PlayerPrefs.GetInt(LossesKey, 0);
        PlayerPrefs.SetInt(LossesKey, currentLosses + 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Get total wins across all sessions.
    /// </summary>
    public static int GetWins()
    {
        return PlayerPrefs.GetInt(WinsKey, 0);
    }

    /// <summary>
    /// Get total losses across all sessions.
    /// </summary>
    public static int GetLosses()
    {
        return PlayerPrefs.GetInt(LossesKey, 0);
    }

    /// <summary>
    /// Clear all persistent stats.
    /// </summary>
    public static void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    #endregion

    #region Session Stats

    /// <summary>
    /// Handle game over - update persistent stats.
    /// </summary>
    public static void HandleGameOver(bool playerWon)
    {
        if (playerWon)
        {
            IncrementWins();
        }
        else
        {
            IncrementLosses();
        }
    }

    /// <summary>
    /// Reset session scores (for new match).
    /// </summary>
    public static void ResetScores()
    {
        playerWins = 0;
        aiWins = 0;
    }

    public static void IncrementPlayerWins()
    {
        playerWins++;
    }

    public static void IncrementAiWins()
    {
        aiWins++;
    }

    public static int GetPlayerWins()
    {
        return playerWins;
    }

    public static int GetAiWins()
    {
        return aiWins;
    }

    #endregion
}
