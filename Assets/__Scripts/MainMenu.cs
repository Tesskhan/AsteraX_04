using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(RectTransform) )]
[RequireComponent( typeof(Image) )]
public class MainMenu : ActiveOnlyDuringSomeGameStates {
    public PlayerShip playerShip;
    public Button startButton;
    void Start()
    {
        if (playerShip != null)
        {
            playerShip.gameObject.SetActive(false); // Disable the PlayerShip
        }
    }
    void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
        }
    }

    public void OnStartButtonPressed()
    {
        Debug.Log("Start button pressed!");

        // Enable the PlayerShip
        if (playerShip != null)
        {
            playerShip.gameObject.SetActive(true);
        }

        // Deactivate the MainMenu
        gameObject.SetActive(false);

        // Change the game state to preLevel
        AsteraX.GAME_STATE = AsteraX.eGameState.preLevel;
    }
}