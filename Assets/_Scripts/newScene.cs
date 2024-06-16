using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class newScene : MonoBehaviour
{
    // The name of the scene to load
    public string sceneName;

    public Button YesButton;

    void Start()
    {
        // Add a listener to the button to call the ChangeScene method when clicked
        YesButton.onClick.AddListener(ChangeScene);

    }

    public void ChangeScene()
    {
        // Load the specified scene
        SceneManager.LoadScene(sceneName);
    }

}
