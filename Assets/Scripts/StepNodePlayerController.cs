using UnityEngine;
using TMPro;

public class StepNodePlayerController : MonoBehaviour
{
    [Header("Current Node")]
    public StepNode currentNode;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float playerHeightOffset = 1f;
    public float arriveDistance = 0.05f;

    [Header("Hotkey Movement")]
    public StepNodeAdjacencyDetector adjacencyDetector;

    [Header("UI Text")]
    public TMP_Text messageText;
    public NotificationUIController notificationUI;

    private StepNode targetNode;
    private bool isMoving = false;
    private StepNode[] allNodes;
    private MazeRunResultTracker resultTracker;
    private Vector3 moveTargetPosition;
    private static readonly KeyCode[] nodeHotkeys =
    {
        KeyCode.A,
        KeyCode.B,
        KeyCode.C,
        KeyCode.D,
        KeyCode.E,
        KeyCode.F,
        KeyCode.G,
        KeyCode.H,
        KeyCode.I,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
        KeyCode.M,
        KeyCode.N
    };

    void Start()
    {
        allNodes = FindObjectsOfType<StepNode>();
        resultTracker = GetComponent<MazeRunResultTracker>();

        if (resultTracker == null)
        {
            resultTracker = gameObject.AddComponent<MazeRunResultTracker>();
        }

        if (adjacencyDetector == null)
        {
            adjacencyDetector = GetComponent<StepNodeAdjacencyDetector>();
        }

        if (adjacencyDetector == null)
        {
            adjacencyDetector = gameObject.AddComponent<StepNodeAdjacencyDetector>();
        }

        string returnNodeName = PlayerPrefs.GetString("ReturnNodeName", "");

        if (!string.IsNullOrEmpty(returnNodeName))
        {
            StepNode returnNode = FindNodeByName(returnNodeName);

            if (returnNode != null)
            {
                currentNode = returnNode;
                PlayerPrefs.DeleteKey("ReturnNodeName");
                PlayerPrefs.DeleteKey("ReturnSceneName");
                PlayerPrefs.Save();
            }
        }

        if (currentNode != null)
        {
            transform.position = GetPlayerPosition(currentNode);
            ShowNodeMessage();
        }
    }

    void OnEnable()
    {
        RefreshCurrentNodeAfterSceneLoad();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveToTargetNode();
            return;
        }

        HandleInput();
    }

    void HandleInput()
    {
        if (currentNode == null)
        {
            return;
        }

        for (int i = 0; i < nodeHotkeys.Length; i++)
        {
            if (Input.GetKeyDown(nodeHotkeys[i]))
            {
                TryMoveToHotkeyNode(i);
                return;
            }
        }
    }

    public void TryMoveToInputLetter(char inputLetter)
    {
        if (isMoving || currentNode == null)
        {
            return;
        }

        char normalizedLetter = char.ToLowerInvariant(inputLetter);
        int hotkeyIndex = normalizedLetter - 'a';

        if (hotkeyIndex < 0 || hotkeyIndex >= nodeHotkeys.Length)
        {
            ShowCannotMoveMessage();
            return;
        }

        TryMoveToHotkeyNode(hotkeyIndex);
    }

    public void TryMoveToInputLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        TryMoveToInputLetter(input.Trim()[0]);
    }

    public void ForceRefreshForIPad()
    {
        allNodes = FindObjectsOfType<StepNode>();

        if (adjacencyDetector == null)
        {
            adjacencyDetector = GetComponent<StepNodeAdjacencyDetector>();
        }

        isMoving = false;
        targetNode = null;
    }

    public void TryMoveToInputLetterFromIPad(char inputLetter)
    {
        ForceRefreshForIPad();

        if (currentNode == null)
        {
            currentNode = FindClosestNodeToPlayer();
        }

        if (currentNode == null)
        {
            Debug.LogWarning("iPad input failed: currentNode is null.");
            return;
        }

        char normalizedLetter = char.ToLowerInvariant(inputLetter);
        int hotkeyIndex = normalizedLetter - 'a';

        StepNode nextNode = FindNodeForHotkeyIndex(hotkeyIndex);

        if (nextNode == null)
        {
            ShowCannotMoveMessage();
            return;
        }

        TryMove(nextNode);
    }

    StepNode FindClosestNodeToPlayer()
    {
        allNodes = FindObjectsOfType<StepNode>();

        StepNode closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (StepNode node in allNodes)
        {
            if (node == null) continue;

            float distance = Vector3.Distance(transform.position, node.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = node;
            }
        }

        return closest;
    }

    void TryMoveToHotkeyNode(int hotkeyIndex)
    {
        StepNode nextNode = FindNodeForHotkeyIndex(hotkeyIndex);

        if (nextNode == null)
        {
            ShowCannotMoveMessage();
            return;
        }

        if (adjacencyDetector != null && !adjacencyDetector.IsAdjacent(currentNode, nextNode))
        {
            ShowCannotMoveMessage();
            return;
        }

        TryMove(nextNode);
    }

    StepNode FindNodeForHotkeyIndex(int hotkeyIndex)
    {
        if (hotkeyIndex >= 0 && hotkeyIndex <= 10)
        {
            return FindNodeByName("StepNode" + (hotkeyIndex + 1));
        }

        if (hotkeyIndex == 11)
        {
            return FindNodeByName("StepNodeS1");
        }

        if (hotkeyIndex == 12)
        {
            return FindNodeByName("StepNodeS2");
        }

        if (hotkeyIndex == 13)
        {
            return FindNodeByName("StepNodeS3");
        }

        return null;
    }

    StepNode FindNodeByName(string nodeName)
    {
        if (allNodes == null || allNodes.Length == 0)
        {
            allNodes = FindObjectsOfType<StepNode>();
        }

        foreach (StepNode node in allNodes)
        {
            if (node != null && node.name == nodeName)
            {
                return node;
            }
        }

        return null;
    }

    void TryMove(StepNode nextNode)
    {
        if (nextNode == null)
        {
            ShowCannotMoveMessage();
            return;
        }

        targetNode = nextNode;
        moveTargetPosition = GetPlayerPosition(targetNode);

        Vector3 direction = moveTargetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }

        isMoving = true;
    }

    void MoveToTargetNode()
    {
        if (targetNode == null)
        {
            isMoving = false;
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 horizontalTargetPosition = new Vector3(
            moveTargetPosition.x,
            currentPosition.y,
            moveTargetPosition.z
        );

        transform.position = Vector3.MoveTowards(
            currentPosition,
            horizontalTargetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 lookDirection = horizontalTargetPosition - currentPosition;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
        }

        if (Vector3.Distance(transform.position, horizontalTargetPosition) <= arriveDistance)
        {
            FinishMove();
        }
    }

    void FinishMove()
    {
        if (targetNode == null)
        {
            return;
        }

        transform.position = moveTargetPosition;
        currentNode = targetNode;
        targetNode = null;
        isMoving = false;

        if (resultTracker != null)
        {
            resultTracker.RecordNodeArrival(currentNode);
        }

        ShowNodeMessage();
        currentNode.SendMessage("OnPlayerArrived", this, SendMessageOptions.DontRequireReceiver);
    }

    Vector3 GetPlayerPosition(StepNode node)
    {
        return node.transform.position + Vector3.up * playerHeightOffset;
    }

    void ShowNodeMessage()
    {
        if (currentNode == null)
        {
            return;
        }

        bool showedMessage = false;

        if (notificationUI != null)
        {
            notificationUI.Show(currentNode);
            showedMessage = true;
        }
        else if (messageText != null)
        {
            messageText.text = currentNode.GetNodeMessageForTrigger();
            showedMessage = true;
        }

        if (showedMessage)
        {
            currentNode.MarkTriggered();
        }
    }

    void ShowCannotMoveMessage()
    {
        if (notificationUI != null)
        {
            notificationUI.ShowSystemMessage("Maze", "You cannot go that way.");
        }
        else if (messageText != null)
        {
            messageText.text = "You cannot go that way.";
        }
    }

    void RefreshCurrentNodeAfterSceneLoad()
    {
        allNodes = FindObjectsOfType<StepNode>();

        string returnNodeName = PlayerPrefs.GetString("ReturnNodeName", "");

        if (!string.IsNullOrEmpty(returnNodeName))
        {
            StepNode returnNode = FindNodeByName(returnNodeName);

            if (returnNode != null)
            {
                currentNode = returnNode;

                transform.position = GetPlayerPosition(currentNode);

                Debug.Log("Restored current node: " + currentNode.name);
            }
        }

        isMoving = false;
        targetNode = null;

        if (adjacencyDetector == null)
        {
            adjacencyDetector = GetComponent<StepNodeAdjacencyDetector>();
        }
    }
}

