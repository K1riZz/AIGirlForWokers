using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YourProject.Dialogue; // 引用我们自己的数据模型
using System.Linq;

/// <summary>
/// Manages all dialogue interactions in the game, acting as a central hub.
/// It coordinates between story-driven conversations, AI-powered dialogues, and incidental chatter.
/// 它是所有对话交互的中心枢纽，协调故事驱动、AI驱动和随机闲聊的对话。
/// </summary>
[DisallowMultipleComponent]
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

    [Tooltip("Reference to the pet's transform. Used as the speaker for barks and AI dialogue. If not set, it will be found by tag.")]
    [SerializeField] private Transform petTransform;

    [Header("Interaction & Idle Dialogue")]
    [Tooltip("Title of the conversation to start when the pet is clicked.")]
    [SerializeField] private string interactionConversation;

    [Tooltip("Titles of conversations for the pet's idle chatter.")]
    [SerializeField] private string[] idleConversations;

    [Tooltip("Minimum time in seconds between idle dialogues.")]
    [SerializeField] private float minIdleTime = 15f;

    [Tooltip("Maximum time in seconds between idle dialogues.")]
    [SerializeField] private float maxIdleTime = 60f;

    // Public properties for other systems to access dependencies
    public StoryManager StoryManager => storyManager;
    public AIDialogueSystem AiDialogueSystem => aiDialogueSystem;

    // 对话状态
    private Conversation m_CurrentConversation;
    private DialogueNode m_CurrentNode;
    private bool m_IsConversationActive;

    public bool IsConversationActive => m_IsConversationActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); // 只销毁这个重复的组件，而不是整个GameObject
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Find references if not set in the inspector
        if (dialogueUI == null)
        {
            dialogueUI = FindObjectOfType<DialogueUI>();
            if (dialogueUI == null) Debug.LogError("DialogueSystem: DialogueUI reference is missing!", this);
        }

        if (storyManager == null)
        {
            storyManager = FindObjectOfType<StoryManager>();
            if (storyManager == null) Debug.LogError("DialogueSystem: StoryManager reference is missing!", this);
        }

        if (aiDialogueSystem == null)
        {
            aiDialogueSystem = FindObjectOfType<AIDialogueSystem>();
            if (aiDialogueSystem == null) Debug.LogError("DialogueSystem: AIDialogueSystem reference is missing!", this);
        }

        // 将对 DialogueUI 的引用传递给 AIDialogueSystem，以简化消息流
        if (aiDialogueSystem != null && dialogueUI != null)
        {
            aiDialogueSystem.Setup(dialogueUI);
        }

        // Find the pet in the scene. A more robust system might use a direct reference.
        if (petTransform == null)
        {
            GameObject petObject = GameObject.FindGameObjectWithTag("Pet");
            if (petObject != null)
            {
                petTransform = petObject.transform;
            }
            else
            {
                Debug.LogWarning("DialogueSystem: Pet Transform is not assigned and no GameObject with the 'Pet' tag was found. Idle and AI dialogues may not work correctly.", this);
            }
        }

        // 将宠物Transform传递给UI，以便UI元素跟随
        if (dialogueUI != null && petTransform != null)
        {
            dialogueUI.SetFollowTarget(petTransform);
        }
    }

    void Start()
    {
        // 游戏开始后，启动闲置对话的循环
        StartCoroutine(IdleDialogueRoutine());
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
        if (m_IsConversationActive) return;

        m_CurrentConversation = DialogueDataManager.Instance.GetConversation(conversationTitle);
        if (m_CurrentConversation != null)
        {
            m_IsConversationActive = true;
            Debug.Log($"Starting story conversation: {conversationTitle}");
            // 从第一个节点开始
            GoToNode(0);
        }
        else
        {
            Debug.LogError($"DialogueSystem: Conversation with ID '{conversationTitle}' not found.", this);
        }
    }

    /// <summary>
    /// Starts a special event-driven conversation (e.g., reminders, time-based events).
    /// </summary>
    /// <param name="conversationTitle">The title of the event conversation.</param>
    public void StartEventConversation(string conversationTitle)
    {
        if (string.IsNullOrEmpty(conversationTitle))
        {
            Debug.LogError("DialogueSystem: Event conversation title is null or empty.", this);
            return;
        }
        // 剧情对话进行中时，不触发轻量级的事件对话
        if (m_IsConversationActive) return;

        var conversation = DialogueDataManager.Instance.GetConversation(conversationTitle);
        if (conversation != null && conversation.nodes.Count > 0)
        {
            // 对于简单的事件对话，我们使用桌面气泡显示第一句话
            Debug.Log($"Starting event dialogue: {conversationTitle}");
            dialogueUI.ShowDesktopBubble(conversation.nodes[0].text, petTransform);
        }
    }

    /// <summary>
    /// Triggers a random idle dialogue/bark from the pet.
    /// </summary>
    public void StartIdleDialogue()
    {
        if (idleConversations == null || idleConversations.Length == 0) return;
        // 剧情对话进行中时，不触发闲聊
        if (m_IsConversationActive) return;

        string conversation = idleConversations[Random.Range(0, idleConversations.Length)];
        // 闲聊也使用事件对话的方式来显示
        StartEventConversation(conversation);
    }

    /// <summary>
    /// Coroutine to periodically trigger idle dialogues.
    /// </summary>
    private IEnumerator IdleDialogueRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(waitTime);
            StartIdleDialogue();
        }
    }

    /// <summary>
    /// Main entry point for the player to start the story mode.
    /// </summary>
    public void EnterStoryMode()
    {
        // 这里可以定义进入剧情模式时默认开始的对话
        // 例如，一个剧情选择的主菜单，或者直接开始第一个任务
        // 我们暂时复用 interactionConversation，你也可以为此创建一个新的字段
        if (!string.IsNullOrEmpty(interactionConversation))
        {
            StartStoryConversation(interactionConversation);
        }
        else
        {
            Debug.LogWarning("DialogueSystem: No default story conversation is set for EnterStoryMode.", this);
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
        else
        {
            Debug.LogWarning("DialogueSystem: 'Interaction Conversation' is not set. Cannot start interaction dialogue.", this);
        }
    }

    /// <summary>
    /// Processes the player's choice and moves to the next node.
    /// </summary>
    public void OnPlayerSelectedChoice(int choiceIndex)
    {
        if (!m_IsConversationActive || m_CurrentNode == null || m_CurrentNode.choices.Count <= choiceIndex)
        {
            return;
        }

        DialogueChoice choice = m_CurrentNode.choices[choiceIndex];
        GoToNode(choice.nextNodeId);
    }

    private void GoToNode(int nodeId)
    {
        // 退出前一个节点
        if (m_CurrentNode != null)
        {
            ProcessNodeEvent(m_CurrentNode.onExit);
        }

        m_CurrentNode = m_CurrentConversation.nodes.Find(n => n.nodeId == nodeId);

        if (m_CurrentNode == null)
        {
            Debug.LogError($"Node with ID {nodeId} not found in conversation {m_CurrentConversation.id}");
            EndConversation();
            return;
        }

        // 进入新节点
        ProcessNodeEvent(m_CurrentNode.onEnter);

        // 更新UI
        dialogueUI.DisplayNode(m_CurrentNode);

        if (m_CurrentNode.isEnd)
        {
            EndConversation();
        }
    }

    private void ProcessNodeEvent(DialogueEvent dialogueEvent)
    {
        if (dialogueEvent == null) return;

        // 设置故事标记
        if (!string.IsNullOrEmpty(dialogueEvent.setStoryFlag))
        {
            storyManager.UpdateStoryState(dialogueEvent.setStoryFlag);
        }

        // 触发全局事件 (需要一个 EventManager)
        // if (!string.IsNullOrEmpty(dialogueEvent.triggerEvent))
        // {
        //     EventManager.TriggerEvent(dialogueEvent.triggerEvent, dialogueEvent.eventData);
        // }
    }

    private void EndConversation()
    {
        if (!m_IsConversationActive) return; // 防止重复调用

        Debug.Log($"Conversation '{m_CurrentConversation.id}' ended.");
        if (storyManager != null)
        {
            storyManager.UpdateStoryState(m_CurrentConversation.id);
        }
        
        ExitStoryMode();
    }

    /// <summary>
    /// Force exits any active story conversation and returns to desktop mode.
    /// </summary>
    public void ExitStoryMode()
    {
        m_IsConversationActive = false;
        m_CurrentConversation = null;
        m_CurrentNode = null;
        dialogueUI.HideStoryPanel(); // 需要在DialogueUI中实现此方法
    }
}
