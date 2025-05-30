using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements (Auto-Detected)")]
    private AsteraX asteraX; // Reference to the main game script
    private GameObject panel;
    private Button startButton;

    void Awake()
    {
        // Assign the panel as this GameObject
        panel = gameObject;

        // Find StartButton and add listener
        Transform buttonTransform = transform.Find("StartButton");
        if (buttonTransform != null)
        {
            startButton = buttonTransform.GetComponent<Button>();
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonPressed);
        }
        else
        {
            Debug.LogWarning("MainMenu: Could not find child named 'StartButton'");
        }
    }

    void Start()
    {
        asteraX = GameObject.Find("Main Camera").GetComponent<AsteraX>();
        // Show the main menu when the game starts
        ShowMenu(true);
    }

    public void OnStartButtonPressed()
    {
        Debug.Log("Start button pressed!");

        // Hide the main menu
        ShowMenu(false);

        // Change game state
        AsteraX.GAME_STATE = AsteraX.eGameState.preLevel;
        asteraX.StartLevelWithLevelIndex();
    }

    private void ShowMenu(bool show)
    {
        if (panel != null)
            panel.SetActive(show);

        if (startButton != null)
            startButton.gameObject.SetActive(show);
    }
}
