using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGame : MonoBehaviour
{
    public void playGame()
    {
        SceneManager.LoadScene("first_scene");
    }
    public void endGame()
    {
        Application.Quit();
    }
}
