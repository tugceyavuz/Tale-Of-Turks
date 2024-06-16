using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.IO;

public class OldWomanNPC : MonoBehaviour
{
    public OnresponseEvent Onresponse;

    [System.Serializable]
    public class OnresponseEvent : UnityEvent<string> { }

    private OpenAIApi openAi;
    private List<ChatMessage> messages = new List<ChatMessage>();

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField playerInputField; // Input field for player input
    public GameObject button;
    public TextMeshProUGUI interactionMessage;

    private string currentResponse = "";

    public float wordSpeed = 0.1f; // Default value, adjust as needed
    private bool playerClose;
    private bool dialogueInProgress; // Flag to indicate if a dialogue is in progress

    void Start()
    {
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        button.SetActive(false);
        interactionMessage.text = ""; // Hide interaction message at start

        Onresponse.AddListener(DisplayResponse); // Add listener for displaying the response

        string openingPromptFilePath = Path.Combine(Application.streamingAssetsPath, "OldWomanNPC.txt");

        // Initialize OpenAIApi with keys from ConfigManager
        Config config = ConfigManager.Instance.Config;
        openAi = new OpenAIApi("sk-proj-CeQ2lL5q0tNHbDllykKqT3BlbkFJGqUrT8xLc8kbItMBUJBZ", "org-rGuNfRYVID6uGkQP4wsLfjQ0");

        // Read and send the opening prompt at the start
        string openingPrompt = ReadOpeningPrompt(openingPromptFilePath);
        if (!string.IsNullOrEmpty(openingPrompt))
        {
            AskChatGpt(openingPrompt);
        }
        else
        {
            Debug.LogError("Failed to read the opening prompt.");
        }

    }

    string ReadOpeningPrompt(string openingPromptFilePath)
    {
        try
        {
            // Read the opening prompt from the text file
            string openingPrompt = File.ReadAllText(openingPromptFilePath);
            return openingPrompt;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error reading the opening prompt: " + ex.Message);
            return null;
        }
    }

    void Update()
    {
        // Check if the player pressed F and is close to the NPC
        if (Input.GetKeyUp(KeyCode.F) && playerClose && !dialogueInProgress)
        {
            interactionMessage.text = ""; // Clear interaction message when F is pressed

            if (dialoguePanel.activeInHierarchy)
            {
                zeroText();
            }
            else
            {
                dialoguePanel.SetActive(true);
                playerInputField.gameObject.SetActive(true); // Show input field
                playerInputField.ActivateInputField(); // Focus on input field
                dialogueInProgress = true; // Set dialogue in progress
            }
        }

        // Check if Enter is pressed while the input field is focused
        if (Input.GetKeyUp(KeyCode.Return) && playerInputField.isFocused && this.CompareTag("OldNPC"))
        {
            string userInput = playerInputField.text;
            playerInputField.text = ""; // Clear the input field

            AskChatGpt(userInput);
            // Do not hide input field here
        }

        // Show or hide the interaction message based on playerClose
        if (playerClose && !dialoguePanel.activeInHierarchy && !dialogueInProgress)
        {
            interactionMessage.text = "Press F to interact";
        }
        else
        {
            interactionMessage.text = "";
        }
    }

    public async void AskChatGpt(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAi.CreateChatCompletion(request);
        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            Onresponse.Invoke(chatResponse.Content);
        }
    }

    void DisplayResponse(string response)
    {
        currentResponse = response;
        StartCoroutine(Typing(currentResponse)); // Start typing the response
    }

    public void zeroText()
    {
        dialogueText.text = "";
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (button != null)
            button.SetActive(false);
        dialogueInProgress = false; // Reset dialogue in progress
    }

    IEnumerator Typing(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
        button.SetActive(true);
    }

    public void NextLine()
    {
        button.SetActive(false);
        // Optionally, you could prompt the player for the next input here
    }

    public void EndInteraction()
    {
        playerClose = false;
        zeroText();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && this.CompareTag("OldNPC"))
        {
            playerClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && this.CompareTag("OldNPC"))
        {
            playerClose = false;
            zeroText();
        }
    }
}
