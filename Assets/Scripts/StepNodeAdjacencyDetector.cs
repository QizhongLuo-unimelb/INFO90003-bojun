using UnityEngine;

public class StepNodeAdjacencyDetector : MonoBehaviour
{
    [Header("Grid Adjacency")]
    public float neighborDistance = 2f;
    public float distanceTolerance = 0.45f;
    public float cardinalAlignmentThreshold = 0.85f;

    public bool IsAdjacent(StepNode currentNode, StepNode targetNode)
    {
        if (currentNode == null || targetNode == null || currentNode == targetNode)
        {
            return false;
        }

        Vector3 offset = targetNode.transform.position - currentNode.transform.position;
        offset.y = 0f;

        float distance = offset.magnitude;

        if (distance < 0.01f)
        {
            return false;
        }

        if (Mathf.Abs(distance - neighborDistance) > distanceTolerance)
        {
            return false;
        }

        Vector3 direction = offset / distance;

        float rightAlignment = Mathf.Abs(Vector3.Dot(direction, Vector3.right));
        float forwardAlignment = Mathf.Abs(Vector3.Dot(direction, Vector3.forward));

        return Mathf.Max(rightAlignment, forwardAlignment) >= cardinalAlignmentThreshold;
    }
}
