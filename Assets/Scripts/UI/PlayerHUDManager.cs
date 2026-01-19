using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the in-game HUD including:
/// - Round win indicators
/// - Match timer
/// - Pause menu
/// - End game screen
/// - Environment switching
/// </summary>
public class PlayerHUDManager : MonoBehaviour
{
    #region Win Indicators

    [Header("Win Indicators")]
    public Image[] winIndicatorsLeft;
    public Image[] winIndicatorsRight;
    public Image[] noneIndicatorsLeft;
    public Image[] noneIndicatorsRight;
    public Color winColor = Color.green;
    public Color defaultColor = Color.gray;
    public Color noneColor = Color.white;

    private int winsLeft = 0;
    private int winsRight = 0;

    #endregion

    #region Action Menu

    [Header("Action Menu")]
    public Button tab;
    public Image imgOpenTab;
    public Image imgCloseTab;
    public RectTransform actionMenu;

    private Vector3 actionMenuClosedPosition;
    private Vector3 actionMenuOpenPosition;
    private bool isTabOpen = false;

    #endregion

    #region Pause Menu

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button btnPause;
    public Button btnContinue;
    public Button btnMenu;

    #endregion

    #region Timer

    [Header("Timer")]
    public TMP_Text timerText;
    private float timeRemaining = 120f;
    private bool isPaused = false;

    #endregion

    #region Environments

    [Header("Environments")]
    public GameObject arenaEnvironment;
    public GameObject hangerEnvironment;
    public GameObject cityEnvironment;

    #endregion

    #region End Game

    [Header("End Game")]
    public Text winText;
    public Text loseText;
    public GameObject endGameMenu;
    public Button returnButton;

    #endregion

    private Vector3 playerInitialPosition;
    private Vector3 aiInitialPosition;
    private HealthManager healthManager;

    void Start()
    {
        InitializeIndicators(winIndicatorsLeft, noneIndicatorsLeft);
        InitializeIndicators(winIndicatorsRight, noneIndicatorsRight);
        InitializeActionMenu();
        InitializePauseMenu();
        InitializeEnvironment();
        InitializePositions();

        healthManager = FindObjectOfType<HealthManager>();

        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        endGameMenu.SetActive(false);

        returnButton.onClick.AddListener(ReturnToMenu);
    }

    void Update()
    {
        if (!isPaused)
        {
            UpdateTimer();
        }
    }

    #region Initialization

    private void InitializeIndicators(Image[] winIndicators, Image[] noneIndicators)
    {
        for (int i = 0; i < winIndicators.Length; i++)
        {
            winIndicators[i].gameObject.SetActive(false);
            noneIndicators[i].color = noneColor;
            noneIndicators[i].gameObject.SetActive(true);
        }
    }

    private void InitializeActionMenu()
    {
        actionMenuClosedPosition = actionMenu.localPosition;
        actionMenuOpenPosition = actionMenuClosedPosition + new Vector3(0, 161, 0);

        tab.onClick.AddListener(ToggleTab);
        imgOpenTab.gameObject.SetActive(true);
        imgCloseTab.gameObject.SetActive(false);
    }

    private void InitializePauseMenu()
    {
        btnPause.onClick.AddListener(OpenPauseMenu);
        btnContinue.onClick.AddListener(ClosePauseMenu);
        btnMenu.onClick.AddListener(ReturnToMenu);
        pauseMenu.SetActive(false);
    }

    private void InitializeEnvironment()
    {
        string selectedEnvironment = PlayerPrefs.GetString("SelectedEnvironment", "Arena");

        arenaEnvironment.SetActive(false);
        hangerEnvironment.SetActive(false);
        cityEnvironment.SetActive(false);

        switch (selectedEnvironment)
        {
            case "Arena":
                arenaEnvironment.SetActive(true);
                break;
            case "Hanger":
                hangerEnvironment.SetActive(true);
                break;
            case "City":
                cityEnvironment.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Unknown environment: {selectedEnvironment}");
                arenaEnvironment.SetActive(true);
                break;
        }
    }

    private void InitializePositions()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ai = GameObject.FindGameObjectWithTag("AI");

        if (player != null)
            playerInitialPosition = player.transform.position;

        if (ai != null)
            aiInitialPosition = ai.transform.position;
    }

    #endregion

    #region Win Tracking

    public void AddWinLeft()
    {
        if (winsLeft < 2)
        {
            winIndicatorsLeft[winsLeft].color = winColor;
            winIndicatorsLeft[winsLeft].gameObject.SetActive(true);
            noneIndicatorsLeft[winsLeft].gameObject.SetActive(false);
        }
        winsLeft++;
        ScoreManager.IncrementPlayerWins();
    }

    public void AddWinRight()
    {
        if (winsRight < 2)
        {
            winIndicatorsRight[winsRight].color = winColor;
            winIndicatorsRight[winsRight].gameObject.SetActive(true);
            noneIndicatorsRight[winsRight].gameObject.SetActive(false);
        }
        winsRight++;
        ScoreManager.IncrementAiWins();
    }

    /// <summary>
    /// Called when a round ends. Updates win indicators and checks for match end.
    /// </summary>
    /// <param name="winner">"Left" for player, "Right" for AI</param>
    public void EndRound(string winner)
    {
        isPaused = true;

        if (winner == "Left")
        {
            AddWinLeft();
        }
        else if (winner == "Right")
        {
            AddWinRight();
        }

        EvaluateGameEnd();
    }

    private void EvaluateGameEnd()
    {
        if (winsLeft >= 2)
        {
            EndGame("Left");
        }
        else if (winsRight >= 2)
        {
            EndGame("Right");
        }
        else
        {
            ResetRound();
        }
    }

    #endregion

    #region Game Flow

    public void EndGame(string winner)
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (winner == "Left")
        {
            winText.gameObject.SetActive(true);
            loseText.gameObject.SetActive(false);
            ScoreManager.HandleGameOver(true);
        }
        else
        {
            loseText.gameObject.SetActive(true);
            winText.gameObject.SetActive(false);
            ScoreManager.HandleGameOver(false);
        }

        timerText.text = "0:00";
        endGameMenu.SetActive(true);
    }

    private void ResetRound()
    {
        isPaused = false;
        Time.timeScale = 1f;
        healthManager.ResetHealth();
        StartCoroutine(ResetCharacterPositions());
        timeRemaining = 120f;
        timerText.text = "2:00";
    }

    private IEnumerator ResetCharacterPositions()
    {
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ai = GameObject.FindGameObjectWithTag("AI");

        if (player != null)
        {
            player.transform.position = playerInitialPosition;
            player.transform.rotation = Quaternion.identity;
        }

        if (ai != null)
        {
            ai.transform.position = aiInitialPosition;
            ai.transform.rotation = Quaternion.identity;
        }
    }

    #endregion

    #region Timer

    private void UpdateTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining);
        }
        else
        {
            // Time ran out - draw
            AddWinLeft();
            AddWinRight();
            EvaluateGameEnd();
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        timeToDisplay = Mathf.Max(0, timeToDisplay);
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0}:{1:00}", (int)minutes, (int)seconds);
    }

    #endregion

    #region Menu Controls

    public void ToggleTab()
    {
        isTabOpen = !isTabOpen;
        actionMenu.localPosition = isTabOpen ? actionMenuOpenPosition : actionMenuClosedPosition;
        imgOpenTab.gameObject.SetActive(!isTabOpen);
        imgCloseTab.gameObject.SetActive(isTabOpen);
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void ReturnToMenu()
    {
        ResetGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene("startScene");
    }

    private void ResetGame()
    {
        // Cleanup persistent objects
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ai = GameObject.FindGameObjectWithTag("AI");

        if (player != null) Destroy(player);
        if (ai != null) Destroy(ai);

        winsLeft = 0;
        winsRight = 0;
        InitializeIndicators(winIndicatorsLeft, noneIndicatorsLeft);
        InitializeIndicators(winIndicatorsRight, noneIndicatorsRight);

        timeRemaining = 120f;
        timerText.text = "2:00";

        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        endGameMenu.SetActive(false);

        ScoreManager.ResetScores();
    }

    #endregion
}
