using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void Start()
    {
        if (aiChatPanel != null)
        {
            aiChatPanel.SetActive(false);
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

    public void ShowAIChatPanel(bool show)
    {
        if (aiChatPanel != null)
        {
            aiChatPanel.SetActive(show);
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

    private IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
