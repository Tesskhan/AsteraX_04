using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : MonoBehaviour
{
    private bool isPaused = false; // Tracks whether the game is paused
    // Start is called before the first frame update
    void Start()
    {
        // Ensure the panel is hidden before Start
        gameObject.SetActive(false);
    }
    public void TogglePause()
    {
        if (isPaused)
        {
            gameObject.SetActive(false); // Hide the pause panel
        }
        else
        {
            gameObject.SetActive(true); // Show the pause panel
        }
    }
}
