using UnityEngine;
using System.Collections.Generic;
using System.IO;
using YourProject.Dialogue; // 引用上面定义的数据模型命名空间

public class DialogueDataManager : MonoBehaviour
{
    public static DialogueDataManager Instance { get; private set; }

    private Dictionary<string, Conversation> m_ConversationDatabase = new Dictionary<string, Conversation>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllConversations();
    }

    public void LoadAllConversations()
    {
        // 我们将所有对话JSON文件放在 "Assets/Resources/Dialogues" 文件夹下
        TextAsset[] dialogueFiles = Resources.LoadAll<TextAsset>("Dialogues");
        
        foreach (var file in dialogueFiles)
        {
            DialogueCollection collection = JsonUtility.FromJson<DialogueCollection>(file.text);
            foreach (var conversation in collection.conversations)
            {
                if (!m_ConversationDatabase.ContainsKey(conversation.id))
                {
                    m_ConversationDatabase.Add(conversation.id, conversation);
                }
                else
                {
                    Debug.LogWarning($"DialogueDataManager: Duplicate conversation ID '{conversation.id}' found in file '{file.name}'.");
                }
            }
        }
        Debug.Log($"Loaded {m_ConversationDatabase.Count} conversations.");
    }

    public Conversation GetConversation(string id)
    {
        m_ConversationDatabase.TryGetValue(id, out var conversation);
        return conversation;
    }
}
