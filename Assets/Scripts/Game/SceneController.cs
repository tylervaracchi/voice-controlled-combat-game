// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows.Speech;
using System.Linq;

/// <summary>
/// Voice-controlled scene navigation.
/// Allows players to navigate menus using voice commands.
/// </summary>
public class SceneController : MonoBehaviour
{
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords;
    private CameraManager cameraManager;

    void Start()
    {
        InitializeKeywords();
        InitializeKeywordRecognizer();
        cameraManager = FindObjectOfType<CameraManager>();
    }

    /// <summary>
    /// Define voice commands for scene navigation.
    /// </summary>
    private void InitializeKeywords()
    {
        keywords = new Dictionary<string, System.Action>
        {
            { "play", PlayGame },
            { "quit", QuitGame }
        };
    }

    private void InitializeKeywordRecognizer()
    {
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;
        keywordRecognizer.Start();
    }

    private void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"Menu voice command: {args.text}");

        if (keywords.TryGetValue(args.text, out var keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    /// <summary>
    /// Load character selection screen.
    /// </summary>
    public void LoadChooseScreen()
    {
        if (cameraManager != null)
        {
            cameraManager.ResetCameraState();
        }
        SceneManager.LoadScene("ChooseScreen");
    }

    /// <summary>
    /// Load main gameplay scene.
    /// </summary>
    public void LoadGameplayScene()
    {
        if (cameraManager != null)
        {
            cameraManager.ResetCameraState();
        }
        SceneManager.LoadScene("GameplayScene");
    }

    /// <summary>
    /// Voice command: Start the game.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("ChooseScene");
    }

    /// <summary>
    /// Voice command: Quit the application.
    /// </summary>
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.OnPhraseRecognized -= OnKeywordsRecognized;
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}
