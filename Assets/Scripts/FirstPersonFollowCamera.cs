using UnityEngine;

public class FirstPersonFollowCamera : MonoBehaviour
{
    public Transform target;

    [Header("First Person Settings")]
    public float eyeHeight = 1.48f;
    public float forwardOffset = 0.12f;
    public float followSpeed = 16f;
    public bool snapToTarget = false;
    public Camera viewCamera;
    public float fieldOfView = 68f;
    public float pitchDegrees = -1f;
    public float rotationSmoothSpeed = 18f;

    [Header("Movement Feel")]
    public float headBobAmount = 0.035f;
    public float headBobSpeed = 7.5f;

    Vector3 lastTargetPosition;
    float bobTimer;

    void LateUpdate()
    {
        if (target == null) return;

        if (viewCamera == null)
        {
            viewCamera = GetComponent<Camera>();
        }

        if (viewCamera != null)
        {
            viewCamera.fieldOfView = fieldOfView;
            viewCamera.nearClipPlane = 0.04f;
        }

        Vector3 flatForward = target.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = transform.forward;
            flatForward.y = 0f;
        }
        flatForward = flatForward.sqrMagnitude > 0.001f ? flatForward.normalized : Vector3.forward;

        float movedDistance = Vector3.Distance(target.position, lastTargetPosition);
        if (movedDistance > 0.001f)
        {
            bobTimer += Time.deltaTime * headBobSpeed;
        }
        else
        {
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * headBobSpeed);
        }

        float bobOffset = Mathf.Sin(bobTimer) * headBobAmount;
        Vector3 targetPosition = target.position + Vector3.up * (eyeHeight + bobOffset) + flatForward * forwardOffset;

        if (snapToTarget)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
        }

        if (flatForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatForward, Vector3.up) * Quaternion.Euler(pitchDegrees, 0f, 0f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }

        lastTargetPosition = target.position;
    }
}
