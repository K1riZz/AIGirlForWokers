using System;
using System.Collections.Generic;

// 放在一个单独的文件里，例如 DialogueData.cs
namespace YourProject.Dialogue
{
    [Serializable]
    public class DialogueEvent
    {
        public string triggerEvent;
        public string eventData;
        public string setStoryFlag;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public int nextNodeId;
    }

    [Serializable]
    public class DialogueNode
    {
        public int nodeId;
        public string speaker;
        public string text;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public DialogueEvent onEnter;
        public DialogueEvent onExit;
        public bool isEnd;
    }

    [Serializable]
    public class Conversation
    {
        public string id;
        public List<DialogueNode> nodes = new List<DialogueNode>();
    }

    [Serializable]
    public class DialogueCollection
    {
        public List<Conversation> conversations = new List<Conversation>();
    }
}
