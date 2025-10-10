using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the game's story progression, tracking unlocked events and conversations.
/// </summary>
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    private HashSet<string> m_UnlockedStoryNodes = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); // 只销毁这个重复的组件，而不是整个GameObject
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    public void UpdateStoryState(string completedConversationTitle)
    {
        if (string.IsNullOrEmpty(completedConversationTitle) || m_UnlockedStoryNodes.Contains(completedConversationTitle))
        {
            return;
        }

        Debug.Log($"StoryManager: '{completedConversationTitle}' conversation completed. Updating story state.");

        m_UnlockedStoryNodes.Add(completedConversationTitle);
        SaveProgress();
    }

    public bool IsStoryNodeUnlocked(string nodeIdentifier)
    {
        return m_UnlockedStoryNodes.Contains(nodeIdentifier);
    }

    public string GetCurrentStoryContext()
    {
        if (m_UnlockedStoryNodes.Count == 0)
        {
            return "The player has just started the game and met the pet for the first time.";
        }

        return "The player has completed the following story points: " + string.Join(", ", m_UnlockedStoryNodes);
    }

    private void SaveProgress()
    {
        string data = string.Join(";", m_UnlockedStoryNodes);
        PlayerPrefs.SetString("StoryProgress", data);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey("StoryProgress"))
        {
            string data = PlayerPrefs.GetString("StoryProgress");
            if (!string.IsNullOrEmpty(data))
            {
                m_UnlockedStoryNodes = new HashSet<string>(data.Split(';').Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
