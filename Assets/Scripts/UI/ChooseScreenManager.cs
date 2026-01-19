// Copyright (c) 2024 Tyler Varacchi. All Rights Reserved.
// This code is proprietary. Unauthorized copying or use is prohibited.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System.Linq;

/// <summary>
/// Voice-controlled character and environment selection screen.
/// Players can navigate using voice commands or button clicks.
/// 
/// Voice commands:
/// - "upper" / "lower" : Change environment
/// - "left" / "right"  : Change character
/// - "back"            : Return to main menu
/// - "fight"           : Start the match
/// </summary>
public class ChooseScreenManager : MonoBehaviour
{
    #region Environment Selection

    [Header("Environment")]
    public Transform environments;
    public Button btnUp;
    public Button btnDown;

    private int envCurrentIndex = 1;
    private bool envIsAnimating = false;
    public float envAnimationDuration = 0.5f;
    private float[] targetYPositions = { 544f, 559f, 578.5f };

    #endregion

    #region Character Selection

    [Header("Characters")]
    public Transform characterGroup;
    public Transform[] characters;
    public Button btnLeft;
    public Button btnRight;
    public Button btnBack;
    public Button btnFight;

    private int charCurrentIndex = 0;
    private bool charIsAnimating = false;
    public float charAnimationDuration = 0.5f;

    #endregion

    #region UI

    [Header("UI")]
    public TMP_Text txtInfo;
    public TMP_Text txtName;

    private string[] characterNames = { "Mike", "Craig", "Worker Bot", "Diesel" };
    private string[] characterDescriptions =
    {
        "Mike is a Titan known for having an unusual battle catalog. A one of a kind bot that learned fighting from accessing the data banks of a secret planet in the Andromeda Galaxy.",
        "Craig doesn't come from Earth, in fact he was built on Kepler-186f. This Titan is modeled after the humanoids who built Craig.",
        "Once a worker bot helping humans build star ships, WB was one of the first Titans to compete in the arena. A seasoned professional who is hard to beat.",
        "She means business, the hardest hitting bot of the bunch. Diesel can gravity punch allowing her to hit harder than a rhino."
    };

    #endregion

    #region Voice Recognition

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    #endregion

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private Vector3 characterGroupInitialPosition;

    void Start()
    {
        ValidateReferences();
        InitializeButtons();
        InitializePositions();
        InitializeVoiceCommands();
        UpdateCharacterPositions();
    }

    #region Initialization

    private void ValidateReferences()
    {
        if (environments == null)
            Debug.LogError("ChooseScreenManager: Environments Transform not assigned");

        if (characterGroup == null)
            Debug.LogError("ChooseScreenManager: Character Group Transform not assigned");

        if (characters == null || characters.Length == 0)
            Debug.LogError("ChooseScreenManager: Characters array empty");

        if (txtName == null || txtInfo == null)
            Debug.LogError("ChooseScreenManager: UI Text components not assigned");
    }

    private void InitializeButtons()
    {
        btnLeft.onClick.AddListener(MoveLeft);
        btnRight.onClick.AddListener(MoveRight);
        btnBack.onClick.AddListener(GoBack);
        btnFight.onClick.AddListener(StartFight);
        btnUp.onClick.AddListener(MoveDown);
        btnDown.onClick.AddListener(MoveUp);
    }

    private void InitializePositions()
    {
        environments.position = new Vector3(
            environments.position.x,
            targetYPositions[envCurrentIndex],
            environments.position.z
        );

        characterGroupInitialPosition = characterGroup.localPosition;

        initialPositions = new Vector3[characters.Length];
        initialRotations = new Quaternion[characters.Length];

        for (int i = 0; i < characters.Length; i++)
        {
            initialPositions[i] = characters[i].localPosition;
            initialRotations[i] = characters[i].localRotation;
        }
    }

    private void InitializeVoiceCommands()
    {
        keywords.Add("upper", MoveUp);
        keywords.Add("lower", MoveDown);
        keywords.Add("left", MoveLeft);
        keywords.Add("right", MoveRight);
        keywords.Add("back", GoBack);
        keywords.Add("fight", StartFight);

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnVoiceCommandRecognized;
        keywordRecognizer.Start();
    }

    private void OnVoiceCommandRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"Selection voice command: {args.text}");

        if (keywords.TryGetValue(args.text, out var action))
        {
            action.Invoke();
        }
    }

    #endregion

    #region Environment Navigation

    void MoveUp()
    {
        if (!envIsAnimating && envCurrentIndex < targetYPositions.Length - 1)
        {
            envCurrentIndex++;
            StartCoroutine(AnimateEnvironment(targetYPositions[envCurrentIndex]));
        }
    }

    void MoveDown()
    {
        if (!envIsAnimating && envCurrentIndex > 0)
        {
            envCurrentIndex--;
            StartCoroutine(AnimateEnvironment(targetYPositions[envCurrentIndex]));
        }
    }

    IEnumerator AnimateEnvironment(float targetYPosition)
    {
        envIsAnimating = true;
        float startPositionY = environments.position.y;
        float elapsedTime = 0f;

        while (elapsedTime < envAnimationDuration)
        {
            float newYPosition = Mathf.Lerp(startPositionY, targetYPosition, elapsedTime / envAnimationDuration);
            environments.position = new Vector3(environments.position.x, newYPosition, environments.position.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        environments.position = new Vector3(environments.position.x, targetYPosition, environments.position.z);
        envIsAnimating = false;
    }

    #endregion

    #region Character Navigation

    void MoveLeft()
    {
        if (!charIsAnimating)
        {
            charCurrentIndex = (charCurrentIndex + 1) % characters.Length;
            StartCoroutine(AnimateCharacters());
        }
    }

    void MoveRight()
    {
        if (!charIsAnimating)
        {
            charCurrentIndex = (charCurrentIndex - 1 + characters.Length) % characters.Length;
            StartCoroutine(AnimateCharacters());
        }
    }

    IEnumerator AnimateCharacters()
    {
        charIsAnimating = true;
        Vector3[] startPositions = new Vector3[characters.Length];
        Quaternion[] startRotations = new Quaternion[characters.Length];
        float elapsedTime = 0f;

        for (int i = 0; i < characters.Length; i++)
        {
            startPositions[i] = characters[i].localPosition;
            startRotations[i] = characters[i].localRotation;
        }

        while (elapsedTime < charAnimationDuration)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                int newIndex = (i - charCurrentIndex + characters.Length) % characters.Length;
                characters[i].localPosition = Vector3.Lerp(startPositions[i], initialPositions[newIndex], elapsedTime / charAnimationDuration);
                characters[i].localRotation = Quaternion.Lerp(startRotations[i], initialRotations[newIndex], elapsedTime / charAnimationDuration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        UpdateCharacterPositions();
        charIsAnimating = false;
    }

    void UpdateCharacterPositions()
    {
        characterGroup.localPosition = characterGroupInitialPosition;

        for (int i = 0; i < characters.Length; i++)
        {
            int newIndex = (i - charCurrentIndex + characters.Length) % characters.Length;
            characters[i].localPosition = initialPositions[newIndex];
            characters[i].localRotation = initialRotations[newIndex];
        }

        // Update UI
        txtName.text = characterNames[charCurrentIndex];
        txtInfo.text = characterDescriptions[charCurrentIndex];
    }

    #endregion

    #region Scene Navigation

    void GoBack()
    {
        SceneManager.LoadScene("startScene");
    }

    void StartFight()
    {
        // Save selected character
        string selectedCharacter = characterNames[charCurrentIndex];
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter);

        // Save selected environment
        float environmentY = environments.position.y;
        string selectedEnvironment = GetEnvironmentNameByY(environmentY);
        PlayerPrefs.SetString("SelectedEnvironment", selectedEnvironment);

        PlayerPrefs.Save();

        SceneManager.LoadScene("GamePlayScene");
    }

    string GetEnvironmentNameByY(float yPosition)
    {
        if (Mathf.Approximately(yPosition, 544f))
            return "Arena";
        else if (Mathf.Approximately(yPosition, 559f))
            return "City";
        else if (Mathf.Approximately(yPosition, 578.5f))
            return "Hanger";

        return "Arena"; // Default
    }

    #endregion

    void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.OnPhraseRecognized -= OnVoiceCommandRecognized;
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}
