using UnityEngine;
using UnityEngine.SceneManagement;

public class NodeScenePortal : MonoBehaviour
{
    [Header("Player")]
    public StepNodePlayerController player;

    [Header("Scene Jump Settings")]
    public string targetSceneName;

    [Header("Return Settings")]
    public string currentSceneName;
    public string returnNodeName;

    [Header("Auto Jump")]
    public bool autoJumpOnArrival = true;

    private StepNode thisNode;

    void Start()
    {
        thisNode = GetComponent<StepNode>();

        if (player == null)
        {
            player = FindObjectOfType<StepNodePlayerController>();
        }
    }

    void OnPlayerArrived(StepNodePlayerController arrivedPlayer)
    {
        if (!autoJumpOnArrival)
        {
            return;
        }

        if (player != null && arrivedPlayer != player)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            return;
        }

        if (!GameRunState.CanEnterBranch(targetSceneName))
        {
            return;
        }

        GameRunState.SaveReturnPoint(currentSceneName, returnNodeName);
        GameRunState.MarkBranchEntered(targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }
}
