using UnityEngine;

public class StepNodeRotateTrigger : MonoBehaviour
{
    public RotatingMazeController rotatingMaze;
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    void OnPlayerArrived(StepNodePlayerController player)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        if (rotatingMaze == null)
        {
            rotatingMaze = FindObjectOfType<RotatingMazeController>();
        }

        if (rotatingMaze == null)
        {
            return;
        }

        hasTriggered = true;
        rotatingMaze.RotateMaze();
    }
}
