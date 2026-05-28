using UnityEngine;

public enum NotificationKind
{
    Auto,
    Email,
    Game,
    Shopping,
    System
}

public class StepNode : MonoBehaviour
{
    [Header("Notification UI")]
    public NotificationKind notificationKind = NotificationKind.Auto;
    public string notificationTitle;

    [Header("Text shown when player reaches this node")]
    [TextArea(3, 6)]
    public string nodeMessage;

    [Header("Text shown after first trigger")]
    public string repeatedNotificationTitle;

    [TextArea(3, 6)]
    public string repeatedNodeMessage;

    private bool hasTriggered = false;

    public bool HasTriggered
    {
        get { return hasTriggered; }
    }

    void Awake()
    {
        hasTriggered = GameRunState.HasVisitedNode(name);
    }

    public string GetNotificationTitleForTrigger()
    {
        if (hasTriggered && !string.IsNullOrWhiteSpace(repeatedNotificationTitle))
        {
            return repeatedNotificationTitle;
        }

        return notificationTitle;
    }

    public string GetInitialNotificationTitle()
    {
        return notificationTitle;
    }

    public string GetNodeMessageForTrigger()
    {
        if (hasTriggered && !string.IsNullOrWhiteSpace(repeatedNodeMessage))
        {
            return repeatedNodeMessage;
        }

        return nodeMessage;
    }

    public string GetInitialNodeMessage()
    {
        return nodeMessage;
    }

    public void MarkTriggered()
    {
        hasTriggered = true;
        GameRunState.MarkNodeVisited(name);
    }
}
