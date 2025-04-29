using UnityEngine;
using UnityEngine.UI;

public class NextLevel : MonoBehaviour
{
    [Header("UI Elements")]
    private Text levelText;              // Text to display the level
    private Button nextLevelButton;      // Button to start the next level

    private AsteraX asteraX;            // Reference to the AsteraX script
    private int currentLevel = 0;       // Tracks the current level

    void Awake()
    {
        // Ensure the panel is hidden before Start
        gameObject.SetActive(false);

        // Find StartButton and add listener
        Transform buttonTransform = transform.Find("NextLevelButton");
        if (buttonTransform != null)
        {
            nextLevelButton = buttonTransform.GetComponent<Button>();
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelPressed);
        }
        else
        {
            Debug.LogWarning("NextLevel: Could not find child named 'NextLevelButton'");
        }
    }

    void Start()
    {
        // Get reference to AsteraX script (assumes it's on Main Camera)
        asteraX = GameObject.Find("Main Camera").GetComponent<AsteraX>();

        // Hook up the button to its click listener
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelPressed);
        }

        UpdateLevelText();
    }

    public void ShowPanel(bool show)
    {
        gameObject.SetActive(show); // Show or hide the panel
    }

    public void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + currentLevel;
        }
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        UpdateLevelText();
    }

    public void OnNextLevelPressed()
    {
        Debug.Log("Next level button pressed!");

        // Hide the panel
        ShowPanel(false);

        // Tell AsteraX to start the next level using currentLevel
        if (asteraX != null)
        {
            asteraX.StartLevelWithLevelIndex(currentLevel);
        }
    }
}
