using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(RectTransform) )]
[RequireComponent( typeof(Image) )]
public class MainMenu : ActiveOnlyDuringSomeGameStates {

    public enum eMainMenuPanelState {
        none, idle, fadeIn, fadeIn2, fadeIn3, display
    }
    
    [Header("Set in Inspector")]
    [Tooltip("The amount of time that it will take for this panel to fade in or fade out.")]
    public float fadeTime = 1f;
    
    [Header("Set Dynamically")]
    [SerializeField]
    private eMainMenuPanelState state = eMainMenuPanelState.none;
    
    Image img;
    Text levelText, infoText;
    Button startButton;
    RectTransform levelRT;//, infoRT; // infoRT was unused, so I've commented it out. – JGB
    float stateStartTime, stateDuration;
    eMainMenuPanelState nextState;
    
    AsteraX.CallbackDelegate displayCallback, idleCallback;
    
    // Use this for initialization
    override public void Awake() {
        img = GetComponent<Image>();

        // Find the LevelText child
        Transform levelT = transform.Find("LevelText");
        if (levelT == null) {
            Debug.LogWarning("MainMenu:Awake() - MainMenu lacks a child named LevelText.");
            return;
        }
        levelRT = levelT.GetComponent<RectTransform>();
        levelText = levelT.GetComponent<Text>();
        if (levelText == null) {
            Debug.LogWarning("MainMenu:Awake() - MainMenu child LevelText needs a Text component.");
            return;
        }

        // Find the InfoText child
        Transform infoT = transform.Find("InfoText");
        if (infoT == null) {
            Debug.LogWarning("MainMenu:Awake() - MainMenu lacks a child named InfoText.");
            return;
        }
        infoText = infoT.GetComponent<Text>();
        if (infoText == null) {
            Debug.LogWarning("MainMenu:Awake() - MainMenu child InfoText needs a Text component.");
            return;
        }

        // Find the StartButton child
        Transform buttonT = transform.Find("StartButton");
        if (buttonT == null) {
            Debug.LogWarning("MainMenu:Awake() - MainMenu lacks a child named StartButton.");
            return;
        }
        startButton = buttonT.GetComponent<Button>();
        if (startButton == null) {
            Debug.LogWarning("MainMenu:Awake() - StartButton needs a Button component.");
            return;
        }

        // Assign the OnClick event
        startButton.onClick.AddListener(OnStartButtonPressed);

        SetState(eMainMenuPanelState.idle);

        base.Awake();
    }

    protected override void DetermineActive()
    {
        base.DetermineActive();
        if (AsteraX.GAME_STATE == AsteraX.eGameState.gameOver) {
            // This should only happen when the game is over
            SetState(eMainMenuPanelState.fadeIn);
        }
    }

    void SetState(eMainMenuPanelState newState)
    {
        stateStartTime = realTime;

        switch (newState)
        {
            case eMainMenuPanelState.idle:
                gameObject.SetActive(false);
                break;

            case eMainMenuPanelState.fadeIn:
                gameObject.SetActive(true);
                img.color = Color.clear;
                stateDuration = fadeTime * 0.2f;
                nextState = eMainMenuPanelState.fadeIn2;
                infoText.text = "Level: "+AsteraX.levelIndex;
                break;

            case eMainMenuPanelState.fadeIn2:
                img.color = Color.black;
                stateDuration = fadeTime * 0.6f;
                nextState = eMainMenuPanelState.fadeIn3;
                break;

            case eMainMenuPanelState.fadeIn3:
                img.color = Color.black;
                stateDuration = fadeTime * 0.2f;
                nextState = eMainMenuPanelState.display;
                break;

            case eMainMenuPanelState.display:
                stateDuration = 999999;
                nextState = eMainMenuPanelState.none;
                break;
        }

        state = newState;
    }

    void Update()
    {
        if (state == eMainMenuPanelState.none) return;

        float u = (realTime - stateStartTime) / stateDuration;
        bool moveNext = false;
        if (u > 1)
        {
            u = 1;
            moveNext = true;
        }

        switch (state)
        {
            case eMainMenuPanelState.fadeIn:
                img.color = new Color(0, 0, 0, u);
                break;

            case eMainMenuPanelState.fadeIn2:
                break;

            case eMainMenuPanelState.fadeIn3:
                break;

            case eMainMenuPanelState.display:
                break;
        }

        if (moveNext)
        {
            SetState(nextState);
        }
    }

    void OnStartButtonPressed()
    {
        Debug.Log("Start button pressed! Changing game state...");

        AsteraX.StartGame();
    }

    float realTime
    {
        get { return Time.realtimeSinceStartup; }
    }
}