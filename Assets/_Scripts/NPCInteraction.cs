using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.IO;

public class NPCInteraction : MonoBehaviour
{
    public OnResponseEvent OnResponse;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    private OpenAIApi openAi;
    private List<ChatMessage> messages = new List<ChatMessage>();

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField playerInputField;
    public GameObject button;
    public TextMeshProUGUI interactionMessage;

    private string currentResponse = "";
    private string openingPromptFilePath; // Path to the opening prompt text file
    private bool dialogueInProgress;

    public float wordSpeed = 0.1f;
    public bool playerClose;

    void Start()
    {
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        button.SetActive(false);
        interactionMessage.text = "";

        OnResponse.AddListener(DisplayResponse);

        // Construct the path to the opening prompt text file
        openingPromptFilePath = Path.Combine(Application.dataPath, "_Scripts/Characters/" + gameObject.name + ".txt");

        // Initialize OpenAIApi with keys from ConfigManager
        Config config = ConfigManager.Instance.Config;
        openAi = new OpenAIApi(config.openAiApiKey, config.openAiOrgId);

        // Read and send the opening prompt at the start
        string openingPrompt = ReadOpeningPrompt();
        if (!string.IsNullOrEmpty(openingPrompt))
        {
            AskChatGpt(openingPrompt);
        }
        else
        {
            Debug.LogError("Failed to read the opening prompt.");
        }

    }

    string ReadOpeningPrompt()
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
        if (Input.GetKeyUp(KeyCode.F) && playerClose && !dialogueInProgress)
        {
            interactionMessage.text = "";

                dialoguePanel.SetActive(true);
                playerInputField.gameObject.SetActive(true);
                playerInputField.ActivateInputField();
                dialogueInProgress = true;
            
        }

        if (Input.GetKeyUp(KeyCode.Return) && playerInputField.isFocused)
        {
            string userInput = playerInputField.text;
            playerInputField.text = "";
            
            AskChatGpt(userInput);
        }

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

            OnResponse.Invoke(chatResponse.Content);
        }
    }

    void DisplayResponse(string response)
    {
        currentResponse = response;
        StartCoroutine(Typing(currentResponse));
    }

    public void zeroText()
    {
        dialogueText.text = "";
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (button != null)
            button.SetActive(false);
        dialogueInProgress = false;
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
    }

    public void EndInteraction()
    {
        playerClose = false;
        zeroText();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerClose = false;
            zeroText();
        }
    }
}
