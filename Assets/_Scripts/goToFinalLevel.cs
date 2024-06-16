using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goToFinalLevel : MonoBehaviour
{
    public PlayerHealth playerHealth; // Reference to the PlayerHealth script
    public GameObject gameOverPanel; // Reference to the Game Over UI panel

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Ensure the panel is initially disabled
        }
    }

    void Update()
    {
        CheckHealth();
    }

    void CheckHealth()
    {
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            PauseGameAndShowPanel();
        }
    }

    void PauseGameAndShowPanel()
    {
        Time.timeScale = 0; // Pause the game
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Show the Game Over UI panel
        }
    }
}
