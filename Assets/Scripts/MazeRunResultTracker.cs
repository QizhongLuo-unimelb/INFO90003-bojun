using UnityEngine;

public class MazeRunResultTracker : MonoBehaviour
{
    [Header("Finish Node")]
    public string finishNodeName = "StepNode11";

    [Header("Ideal Result")]
    public int idealPathSteps = 10;

    [Header("Result Message")]
    public string resultTitle = "Maze Result";

    [TextArea(2, 4)]
    public string closingQuestion = "Is this your ideal result?\nOr have you simply taken another path?";

    private int actualSteps;

    public int ActualSteps
    {
        get { return actualSteps; }
    }

    void Awake()
    {
        GameRunState.EnsureRunStarted();
        actualSteps = GameRunState.GetMainActualSteps();
    }

    public void RecordNodeArrival(StepNode node)
    {
        if (node == null)
        {
            return;
        }

        actualSteps = GameRunState.RecordMainStep(idealPathSteps);

        if (node.name == finishNodeName)
        {
            ApplyResultMessage(node);
        }
    }

    void ApplyResultMessage(StepNode node)
    {
        string message = BuildResultMessage();

        node.notificationKind = NotificationKind.System;
        node.notificationTitle = resultTitle;
        node.nodeMessage = message;
        node.repeatedNotificationTitle = resultTitle;
        node.repeatedNodeMessage = message;
    }

    string BuildResultMessage()
    {
        int timeTakenSeconds = Mathf.RoundToInt(GameRunState.GetRunElapsedSeconds());
        int extraSteps = Mathf.Max(0, actualSteps - idealPathSteps);

        return "Congratulations—you've escaped the maze.\n"
            + "Time taken: " + timeTakenSeconds + " seconds\n"
            + "Actual steps: " + actualSteps + " steps\n"
            + "Ideal path: " + idealPathSteps + " steps\n"
            + "Extra steps: " + extraSteps + " steps\n"
            + closingQuestion;
    }
}
