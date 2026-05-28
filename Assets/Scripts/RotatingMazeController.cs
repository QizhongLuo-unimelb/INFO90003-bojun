using UnityEngine;

public class RotatingMazeController : MonoBehaviour
{
    public Transform rotatingGroup;
    public Renderer[] ledBorders;
    public float rotationDegrees = -90f;
    public float rotationSpeed = 180f;
    public bool persistRotationInRun = true;

    private Quaternion targetRotation;
    private bool isRotating = false;

    public bool HasPersistedRunRotation
    {
        get
        {
            return persistRotationInRun && GameRunState.GetMainMazeRotationCount() > 0;
        }
    }

    void Awake()
    {
        if (rotatingGroup == null)
        {
            rotatingGroup = transform;
        }

        if (persistRotationInRun)
        {
            int rotationCount = GameRunState.GetMainMazeRotationCount();
            if (rotationCount > 0)
            {
                rotatingGroup.rotation = Quaternion.AngleAxis(
                    rotationDegrees * rotationCount,
                    Vector3.up
                ) * rotatingGroup.rotation;
            }
        }

        targetRotation = rotatingGroup.rotation;
        SetLedBorderLit(false);
    }

    void Update()
    {
        if (!isRotating || rotatingGroup == null)
        {
            return;
        }

        rotatingGroup.rotation = Quaternion.RotateTowards(
            rotatingGroup.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(rotatingGroup.rotation, targetRotation) <= 0.01f)
        {
            rotatingGroup.rotation = targetRotation;
            isRotating = false;
        }
    }

    public void RotateMaze()
    {
        if (rotatingGroup == null)
        {
            return;
        }

        if (persistRotationInRun)
        {
            GameRunState.AdvanceMainMazeRotationCount();
        }

        targetRotation = Quaternion.AngleAxis(rotationDegrees, Vector3.up) * rotatingGroup.rotation;
        isRotating = true;
        SetLedBorderLit(true);
    }

    void SetLedBorderLit(bool lit)
    {
        if (ledBorders == null)
        {
            return;
        }

        Color color = lit ? Color.yellow : new Color(0.35f, 0.32f, 0.18f);

        foreach (Renderer border in ledBorders)
        {
            if (border == null)
            {
                continue;
            }

            border.material.color = color;
        }
    }
}
