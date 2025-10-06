using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YourProject.Dialogue; // 引用数据模型
using System.Collections.Generic;

/// <summary>
/// Manages the user interface for all dialogue, including standard conversations and AI chat.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("AI Chat Panel")]
    [SerializeField] private GameObject aiChatPanel;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private TextMeshProUGUI chatHistoryText;

    [Header("Desktop Bubble Panel")]
    [SerializeField] private GameObject desktopBubblePanel;
    [SerializeField] private TextMeshProUGUI desktopBubbleText;
    [SerializeField] private float bubbleDisplayTime = 4f; // 气泡默认显示时间

    [Header("Story Dialogue Panel")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TextMeshProUGUI storySpeakerText;
    [SerializeField] private TextMeshProUGUI storyLineText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private Button closeStoryPanelButton;

    [Header("Mode Entry")]
    [SerializeField] private GameObject storyModeButton;
    [SerializeField] private Transform petTransform; // 用于按钮跟随

    private Coroutine m_BubbleCoroutine;
    private List<GameObject> m_CurrentChoiceButtons = new List<GameObject>();

    void Start()
    {
        if (aiChatPanel != null)
        {
            aiChatPanel.SetActive(false);
        }
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
        if (desktopBubblePanel != null)
        {
            desktopBubblePanel.SetActive(false);
        }

        if (storyModeButton != null)
        {
            var button = storyModeButton.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnStoryModeButtonClicked);
        }

        if (closeStoryPanelButton != null)
        {
            closeStoryPanelButton.onClick.AddListener(OnCloseStoryPanelClicked);
        }

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendAIChat);
        }

        if (playerInputField != null)
        {
            playerInputField.onSubmit.AddListener((text) => OnSendAIChat());
        }
    }

    void LateUpdate()
    {
        // 让剧情模式按钮跟随宠物
        if (storyModeButton != null && storyModeButton.activeInHierarchy && petTransform != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(petTransform.position);
            storyModeButton.transform.position = screenPos;
        }
    }

    public void SetFollowTarget(Transform target)
    {
        petTransform = target;
    }

    public void ShowAIChatPanel(bool show)
    {
        if (aiChatPanel != null)
        {
            aiChatPanel.SetActive(show);
        }
    }

    public void HideStoryPanel()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
    }

    public void AddAIChatMessage(string speaker, string message)
    {
        if (chatHistoryText != null)
        {
            chatHistoryText.text += $"\n<b>{speaker}:</b> {message}";
            StartCoroutine(ForceScrollDown());
        }
    }

    /// <summary>
    /// 以桌面气泡形式显示单行对话（用于闲置、提醒等）。
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="target">气泡要跟随的目标 (宠物)</param>
    public void ShowDesktopBubble(string message, Transform target)
    {
        if (desktopBubblePanel == null || desktopBubbleText == null) return;

        // 如果上一个气泡还在，先停掉
        if (m_BubbleCoroutine != null)
        {
            StopCoroutine(m_BubbleCoroutine);
        }

        m_BubbleCoroutine = StartCoroutine(ShowBubbleRoutine(message, target));
    }

    private IEnumerator ShowBubbleRoutine(string message, Transform target)
    {
        desktopBubblePanel.SetActive(true);
        desktopBubbleText.text = message;

        // 只要气泡是激活的，就每帧更新它的位置
        while (desktopBubblePanel.activeInHierarchy)
        {
            if (target != null)
            {
                // 将宠物的世界坐标转换为屏幕坐标，然后设置给UI
                Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
                desktopBubblePanel.transform.position = screenPos;
            }
            yield return null;
        }
    }

    public void HideDesktopBubble()
    {
        if (m_BubbleCoroutine != null) StopCoroutine(m_BubbleCoroutine);
        if (desktopBubblePanel != null) desktopBubblePanel.SetActive(false);
    }

    /// <summary>
    /// 在剧情对话面板中显示一个完整的对话节点（包含文本和选项）。
    /// </summary>
    public void DisplayNode(DialogueNode node)
    {
        if (storyPanel == null) return;

        storyPanel.SetActive(true);
        storySpeakerText.text = node.speaker;
        storyLineText.text = node.text;

        // 进入剧情对话时，隐藏桌面上的剧情入口按钮
        if (storyModeButton != null)
            storyModeButton.SetActive(false);

        ClearChoices();

        // 显示剧情对话时，应隐藏桌面气泡，避免重叠
        HideDesktopBubble();

        for (int i = 0; i < node.choices.Count; i++)
        {
            GameObject buttonGO = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            var buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            var button = buttonGO.GetComponent<Button>();

            if (buttonText != null) buttonText.text = node.choices[i].text;
            
            int choiceIndex = i; // 捕获循环变量
            if (button != null)
            {
                button.onClick.AddListener(() => DialogueSystem.Instance.OnPlayerSelectedChoice(choiceIndex));
            }
            m_CurrentChoiceButtons.Add(buttonGO);
        }
    }

    private void OnStoryModeButtonClicked()
    {
        DialogueSystem.Instance.EnterStoryMode();
    }

    private void OnCloseStoryPanelClicked()
    {
        // 无论对话是否结束，都强制退出剧情模式
        DialogueSystem.Instance.ExitStoryMode();
        // 退出剧情模式后，重新显示剧情入口按钮
        if (storyModeButton != null)
            storyModeButton.SetActive(true);
    }

    private void OnSendAIChat()
    {
        if (playerInputField != null && !string.IsNullOrWhiteSpace(playerInputField.text))
        {
            string playerInput = playerInputField.text;
            AddAIChatMessage("You", playerInput);

            DialogueSystem.Instance.AiDialogueSystem.SendPlayerInput(playerInput);

            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }
    }

    private void ClearChoices()
    {
        foreach (var button in m_CurrentChoiceButtons)
        {
            Destroy(button);
        }
        m_CurrentChoiceButtons.Clear();
    }

    private IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
