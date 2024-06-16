using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    // The name of the scene to load
    public string sceneName;
    public GameObject PopUpPanel;

    public Button YesButton;
    public Button NoButton;

    void Start()
    {
        PopUpPanel.SetActive(false);

        // Add a listener to the button to call the ChangeScene method when clicked
        YesButton.onClick.AddListener(ChangeScene);

        // Add a listener to the button to call the ClosePopUp method when clicked
        NoButton.onClick.AddListener(ClosePopUp);
    }

    private void ClosePopUp()
    {
        PopUpPanel.SetActive(false);
    }

    public void ChangeScene()
    {
        // Load the specified scene
        SceneManager.LoadScene(sceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PopUpPanel.SetActive(true);
        }
    }
}
