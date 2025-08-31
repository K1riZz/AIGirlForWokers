using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

/// <summary>
/// Manages all dialogue interactions in the game, acting as a central hub.
/// It coordinates between story-driven conversations, AI-powered dialogues, and incidental chatter.
/// </summary>
[RequireComponent(typeof(DialogueSystemController))] // Ensures the core Dialogue System component is present.
public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("System References")]
    [Tooltip("Reference to the Dialogue UI manager.")]
    [SerializeField] private DialogueUI dialogueUI;

    [Tooltip("Reference to the Story Manager for quest and progression tracking.")]
    [SerializeField] private StoryManager storyManager;

    [Tooltip("Reference to the AI Dialogue System for LLM integration.")]
    [SerializeField] private AIDialogueSystem aiDialogueSystem;

    [Header("Dialogue Configuration")]
    [Tooltip("Title of the conversation to start when the pet is clicked.")]
    [SerializeField] private string interactionConversation;

    [Tooltip("Titles of conversations for the pet's idle chatter.")]
    [SerializeField] private string[] idleConversations;

    // Public properties for other systems to access dependencies
    public StoryManager StoryManager => storyManager;
    public AIDialogueSystem AiDialogueSystem => aiDialogueSystem;

    private Transform petTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find references if not set in the inspector
        if (dialogueUI == null)
            dialogueUI = FindObjectOfType<DialogueUI>();
        if (storyManager == null)
            storyManager = FindObjectOfType<StoryManager>();
        if (aiDialogueSystem == null)
            aiDialogueSystem = FindObjectOfType<AIDialogueSystem>();
        
        // Find the pet in the scene. A more robust system might use a direct reference.
        GameObject petObject = GameObject.FindGameObjectWithTag("Pet");
        if (petObject != null)
        {
            petTransform = petObject.transform;
        }
        else
        {
            Debug.LogWarning("DialogueSystem: Could not find a GameObject with the 'Pet' tag. Idle dialogues may not work correctly.");
        }
    }

    void OnEnable()
    {
        DialogueManager.instance.conversationEnded += OnConversationEnded;
    }

    void OnDisable()
    {
        DialogueManager.instance.conversationEnded -= OnConversationEnded;
    }

    /// <summary>
    /// Starts a story-driven conversation from the Dialogue System database.
    /// </summary>
    /// <param name="conversationTitle">The title of the conversation.</param>
    public void StartStoryConversation(string conversationTitle)
    {
        if (string.IsNullOrEmpty(conversationTitle))
        {
            Debug.LogError("DialogueSystem: Conversation title is null or empty.", this);
            return;
        }
        DialogueManager.StartConversation(conversationTitle);
    }

    /// <summary>
    /// Triggers a random idle dialogue/bark from the pet.
    /// </summary>
    public void StartIdleDialogue()
    {
        if (idleConversations == null || idleConversations.Length == 0) return;
        
        string conversation = idleConversations[Random.Range(0, idleConversations.Length)];
        if (!string.IsNullOrEmpty(conversation) && petTransform != null)
        {
            DialogueManager.Bark(conversation, petTransform);
        }
    }

    /// <summary>
    /// Starts a dialogue when the user interacts with the pet (e.g., clicks on it).
    /// </summary>
    public void StartInteractionDialogue()
    {
        if (!string.IsNullOrEmpty(interactionConversation))
        {
            StartStoryConversation(interactionConversation);
        }
    }

    /// <summary>
    /// Called when a standard Dialogue System conversation ends.
    /// </summary>
    private void OnConversationEnded(Transform conversationTransform)
    {
        Debug.Log("Conversation ended.");
        if (storyManager != null)
        {
            string lastConversation = DialogueManager.lastConversationStarted;
            storyManager.UpdateStoryState(lastConversation);
        }
    }

    internal void displayDialogue(string petName, string simulatedResponse)
    {
        throw new System.NotImplementedException();
    }
}
