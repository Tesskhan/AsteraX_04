﻿//#define DEBUG_AsteraX_LogMethods
//#define DEBUG_AsteraX_RespawnNotifications

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteraX : MonoBehaviour
{

    public GameObject nextLevelPanel;
    public GameObject pausePanel;
    
    // Private Singleton-style instance. Accessed by static property S later in script
    static private AsteraX _S;

    static List<Asteroid>           ASTEROIDS;
    static List<Bullet>             BULLETS;
    static private eGameState       _GAME_STATE = eGameState.mainMenu;
    
	// If you use a fully-qualified class name like this, you don't need "using UnityEngine.UI;" above.
    static UnityEngine.UI.Text  	SCORE_GT;
    // This is an automatic property
    public static int           	SCORE { get; private set; }
    
    public const float MIN_ASTEROID_DIST_FROM_PLAYER_SHIP = 5;
    const float DELAY_BEFORE_RELOADING_SCENE = 4;
    private float currentLevelIndex = 1f;

	public delegate void CallbackDelegate(); // Set up a generic delegate type.
    static public CallbackDelegate GAME_STATE_CHANGE_DELEGATE;
    
	public delegate void CallbackDelegateV3(Vector3 v); // Set up a Vector3 delegate type.

    // System.Flags changes how eGameStates are viewed in the Inspector and lets multiple 
    //  values be selected simultaneously (similar to how Physics Layers are selected).
    // It's only valid for the game to ever be in one state, but I've added System.Flags
    //  here to demonstrate it and to make the ActiveOnlyDuringSomeGameStates script easier
    //  to view and modify in the Inspector.
    // When you use System.Flags, you still need to set each enum value so that it aligns 
    //  with a power of 2. You can also define enums that combine two or more values,
    //  for example the all value below that combines all other possible values.
    [System.Flags]
    public enum eGameState
    {
        // Decimal      // Binary
        none = 0,       // 00000000
        mainMenu = 1,   // 00000001
        preLevel = 2,   // 00000010
        level = 4,      // 00000100
        postLevel = 8,  // 00001000
        gameOver = 16,  // 00010000
        all = 0xFFFFFFF // 11111111111111111111111111111111
    }

    [Header("Set in Inspector")]
    [Tooltip("This sets the AsteroidsScriptableObject to be used throughout the game.")]
    public AsteroidsScriptableObject asteroidsSO;
    private NextLevel nextLevel; // Reference to the NextLevel script

    [Header("This will be set by Remote Settings")]
    public string levelProgression = "1:3/2,2:4/2,3:3/3,4:4/3,5:5/3,6:3/4,7:4/4,8:5/4,9:6/4,10:3/5";

    [Header("These reflect static fields and are otherwise unused")]
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
        + "GAME_STATE_CHANGE_DELEGATE whenever GAME_STATE changes.")]
    protected eGameState  _gameState;

    private bool isPaused = false; // Track whether the game is paused

    private void Awake()
    {
    #if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:Awake()");
    #endif

        ASTEROIDS = new List<Asteroid>();
        BULLETS = new List<Bullet>();

        S = this;

        // Ensure nextLevel is assigned here
        if (nextLevelPanel != null)
        {
            nextLevel = nextLevelPanel.GetComponent<NextLevel>();
        }
        else
        {
            Debug.LogError("NextLevelPanel is not assigned in the Inspector.");
        }

        GAME_STATE_CHANGE_DELEGATE += delegate ()
        {
            this._gameState = AsteraX.GAME_STATE;
            S._gameState = AsteraX.GAME_STATE;
        };

        _gameState = eGameState.mainMenu;
        GAME_STATE = _gameState;
    }

    private void OnDestroy()
    {
        AsteraX.GAME_STATE = AsteraX.eGameState.none;
    }

    private void Start()
    {
        // Initialize the game state to main menu
        GAME_STATE = eGameState.mainMenu;

    }

    public void StartLevelWithLevelIndex()
    {
        if (nextLevel == null)
        {
            Debug.LogWarning("NextLevel reference is not set!");
            return;
        }
        currentLevelIndex += 0.5f;
        Debug.Log($"Starting level {currentLevelIndex}...");
        string[] levels = levelProgression.Split(',');

        if (currentLevelIndex - 1 >= levels.Length)
        {
            Debug.LogWarning("Level index out of range!");
            return;
        }

        string[] parts = levels[(int)currentLevelIndex - 1].Split(':');
        if (parts.Length != 2)
        {
            Debug.LogWarning("Invalid level format!");
            return;
        }

        string[] asteroidData = parts[1].Split('/');
        if (asteroidData.Length != 2)
        {
            Debug.LogWarning("Invalid asteroid data!");
            return;
        }

        int asteroidCount = int.Parse(asteroidData[0]);
        int childrenPerAsteroid = int.Parse(asteroidData[1]);

        Debug.Log($"Starting level {currentLevelIndex} with {asteroidCount} asteroids, {childrenPerAsteroid} children each");

        // Pass these parameters to your level spawning logic
        StartLevel(asteroidCount, childrenPerAsteroid);

        // Optionally update current level in NextLevel script
        nextLevel.SetCurrentLevel((int)currentLevelIndex);
    }

    public void StartLevel(int asteroidCount, int childrenPerAsteroid)
    {
        // Clear and spawn asteroids using static method
        ASTEROIDS.Clear();
        BULLETS.Clear();
        foreach (Asteroid a in FindObjectsOfType<Asteroid>())
        {
            Destroy(a.gameObject);
        }
        foreach (Bullet b in FindObjectsOfType<Bullet>())
        {
            Destroy(b.gameObject);
        }
        Asteroid.SpawnAsteroids(asteroidCount, childrenPerAsteroid);

        Debug.Log($"Spawned {asteroidCount} asteroids with {childrenPerAsteroid} children each.");
    }
    
	public void EndGame()
    {
        GAME_STATE = eGameState.gameOver;
        Invoke("ReloadScene", DELAY_BEFORE_RELOADING_SCENE);
    }

    void ReloadScene()
    {
        // Reload the scene to restart the game
        // Note: This exposes a long-time Unity bug where reloading the scene 
        //  during gameplay within the Editor causes the lighting to all go 
        //  dark and the engine to think that it needs to rebuild the lighting.
        //  This bug does not cause any issues outside of the Editor.
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void Update()
    {
        // Check for the P key press to toggle pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            pausePanel.SetActive(!pausePanel.activeSelf); // Toggle the pause panel visibility
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            // Unpause the game
            Time.timeScale = 1f;
            isPaused = false;
            Debug.Log("Game unpaused");
        }
        else
        {
            // Pause the game
            Time.timeScale = 0f;
            isPaused = true;
            Debug.Log("Game paused");
        }
    }


    // ---------------- Static Section ---------------- //

    /// <summary>
    /// <para>This static private property provides some protection for the Singleton _S.</para>
    /// <para>get {} does return null, but throws an error first.</para>
    /// <para>set {} allows overwrite of _S by a 2nd instance, but throws an error first.</para>
    /// <para>Another advantage of using a property here is that it allows you to place
    /// a breakpoint in the set clause and then look at the call stack if you fear that 
    /// something random is setting your _S value.</para>
    /// </summary>
    static private AsteraX S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AsteraX:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AsteraX:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }


    static public AsteroidsScriptableObject AsteroidsSO
    {
        get
        {
            if (S != null)
            {
                return S.asteroidsSO;
            }
            return null;
        }
    }


    static public eGameState GAME_STATE
    {
        get
        {
            return _GAME_STATE;
        }
        set
        {
            if (value != _GAME_STATE)
            {
                _GAME_STATE = value;
                // Need to update all of the handlers
                // Any time you use a delegate, you run the risk of it not having any handlers
                //  assigned to it. In that case, it is null and will throw a null reference
                //  exception if you try to call it. So *any* time you call a delegate, you 
                //  should check beforehand to make sure it's not null.
                if (GAME_STATE_CHANGE_DELEGATE != null)
                {
                    GAME_STATE_CHANGE_DELEGATE();
                }
            }
        }
    }

    static public int GAME_LEVEL
    {
        get
        {
            return (int)S.currentLevelIndex; // Return the current level index
        }
    }

    
	static public void AddAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) == -1)
        {
            ASTEROIDS.Add(asteroid);
        }
    }

    static public void RefreshAsteroidList()
    {
        ASTEROIDS = new List<Asteroid>(GameObject.FindObjectsOfType<Asteroid>());
    }

    static public void RemoveAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.Contains(asteroid))
        {
            ASTEROIDS.Remove(asteroid);
        }

        RefreshAsteroidList();

        if (ASTEROIDS.Count == 0)
        {
            if (_S.currentLevelIndex >= 10)
            {
                GameOver(); // Trigger game over
                return;
            }

            _S.nextLevel = _S.nextLevelPanel.GetComponent<NextLevel>();
            GAME_STATE = eGameState.postLevel;
            _S.nextLevel.ShowPanel(true);
        }
    }
    
    static public void GameOver()
    {
        _S.EndGame();
    }
    
    
	static public void AddScore(int num)
    {
        // Find the ScoreGT Text field only once.
        if (SCORE_GT == null)
        {
            GameObject go = GameObject.Find("ScoreGT");
            if (go != null)
            {
                SCORE_GT = go.GetComponent<UnityEngine.UI.Text>();
            }
            else
            {
                Debug.LogError("AsteraX:AddScore() - Could not find a GameObject named ScoreGT.");
                return;
            }
            SCORE = 0;
        }
        // SCORE holds the definitive score for the game.
        SCORE += num;

        // Show the score on screen. For info on numeric formatting like "N0", see:
        //  https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        SCORE_GT.text = SCORE.ToString("N0");
    }


    const int RESPAWN_DIVISIONS = 8;
    const int RESPAWN_AVOID_EDGES = 2; // Note: This number must be greater than 0!
    static Vector3[,] RESPAWN_POINTS;
    /// <summary>
    /// <para>Given the point of the PlayerShip when it hit an Asteroid, this method
    /// chooses a respawn point. The RESPAWN_POINT_GRID_DIVISIONS above determines
    /// how many points the game will check. If that number is 8, then the game 
    /// will check 49 (7x7) points within the play area (dividing each dimension
    /// into 8ths and avoiding the edges of the play area).</para>
    /// <para>This method will not find and avoid the location closest to the 
    /// PlayerShip's previous location and then will iterate through all points
    /// and all Asteroids.</para>
    /// <para>This process is not very performant (though given the
    /// small numbers of objects, it's still really fast), so we'll have it use 
    /// a coroutine to demonstrate their use.</para>
    /// </summary>
    /// <returns>The respawn point for the PlayerShip.</returns>
    /// <param name="prevPos">Previous position of the PlayerShip.</param>
    /// <param name="callback">Method to be called when this method is finished.</param>
    static public IEnumerator FindRespawnPointCoroutine(Vector3 prevPos, CallbackDelegateV3 callback)
    {
# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine( "+prevPos+", [CallbackDelegateV3] )");
#endif
        // Spawn particle effect for disappearing
        Instantiate(PlayerShip.DISAPPEAR_PARTICLES, prevPos, Quaternion.identity);

        // Set up the RESPAWN_POINTS once
        if (RESPAWN_POINTS == null)
        {
            RESPAWN_POINTS = new Vector3[RESPAWN_DIVISIONS + 1, RESPAWN_DIVISIONS + 1];
            Bounds playAreaBounds = ScreenBounds.BOUNDS;
            float dX = playAreaBounds.size.x / RESPAWN_DIVISIONS;
            float dY = playAreaBounds.size.y / RESPAWN_DIVISIONS;
            for (int i = 0; i <= RESPAWN_DIVISIONS; i++)
            {
                for (int j = 0; j <= RESPAWN_DIVISIONS; j++)
                {
                    RESPAWN_POINTS[i, j] = new Vector3(
                        playAreaBounds.min.x + i * dX,
                        playAreaBounds.min.y + j * dY,
                        0);
                }
            }
        }

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() yielding for "+PlayerShip.RESPAWN_DELAY+" seconds.");
#endif

        // Wait a few seconds before choosing the nextPos
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.8f);

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() back from yield.");
#endif

        float distSqr, closestDistSqr = float.MaxValue;
        int prevI = 0, prevJ = 0;

        // Check points against prevPos (avoiding edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                // sqrMagnitude avoids doing a needless (and costly) square root operation
                distSqr = (RESPAWN_POINTS[i, j] - prevPos).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    prevI = i;
                    prevJ = j;
                }
            }
        }

        float furthestDistSqr = 0;
        Vector3 nextPos = prevPos;
        // Now, iterate through each of the RESPAWN_POINTS to find the one with 
        //  the largest distance to the closest Asteroid (again avoid edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                if (i == prevI && j == prevJ)
                {
                    continue;
                }
                closestDistSqr = float.MaxValue;
                // Find distance to the closest Asteroid
                for (int k = 0; k < ASTEROIDS.Count; k++)
                {
                    distSqr = (ASTEROIDS[k].transform.position - RESPAWN_POINTS[i, j]).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                    }
                }

                // If this is further than before, this is the best spawn loc
                if (closestDistSqr > furthestDistSqr)
                {
                    furthestDistSqr = closestDistSqr;
                    nextPos = RESPAWN_POINTS[i, j];
                }
            }
        }

        // Spawn particle effect for appearing
        Instantiate(PlayerShip.APPEAR_PARTICLES, nextPos, Quaternion.identity);

        // Give the particle effect just a bit of time before the ship respawns
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.2f);

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() calling back!");
#endif
        callback(nextPos);
    }

}
