using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles communication with a Large Language Model (LLM) for AI-driven dialogue.
/// </summary>
public class AIDialogueSystem : MonoBehaviour
{
    // It's better to manage configuration in a central place.
    [SerializeField] private Config gameConfig;

    /// <summary>
    /// Receives player input and starts the process of getting a response from the LLM.
    /// </summary>
    /// <param name="playerInput">The text the player entered.</param>
    public void SendPlayerInput(string playerInput)
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig is not assigned in AIDialogueSystem.", this);
            return;
        }

        string storyContext = "";
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.StoryManager != null)
        {
            storyContext = DialogueSystem.Instance.StoryManager.GetCurrentStoryContext();
        }

        string prompt = $"Story context: {storyContext}\n\nPlayer: {playerInput}\n{gameConfig.PetName}:";
        
        StartCoroutine(GetAIResponse(prompt));
    }

    private IEnumerator GetAIResponse(string prompt)
    {
        Debug.Log($"Sending prompt to AI: {prompt}");

        // --- Example using UnityWebRequest for a real API call ---
        // string jsonPayload = "{\"prompt\": \"" + prompt + "\", \"max_tokens\": 50}";
        // byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        //
        // using (UnityWebRequest request = new UnityWebRequest(gameConfig.ApiEndpoint, "POST"))
        // {
        //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //     request.downloadHandler = new DownloadHandlerBuffer();
        //     request.SetRequestHeader("Content-Type", "application/json");
        //     request.SetRequestHeader("Authorization", "Bearer " + gameConfig.ApiKey);
        //
        //     yield return request.SendWebRequest();
        //
        //     if (request.result != UnityWebRequest.Result.Success) { ... }
        //     else { ... }
        // }

        // Simulate a network delay and response for development
        yield return new WaitForSeconds(1.5f);
        string simulatedResponse = $"I'm not sure what to say about that, but it's interesting!";
        
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.AiDialogueSystem.BroadcastMessage(gameConfig.PetName, simulatedResponse);
        }
    }
}
