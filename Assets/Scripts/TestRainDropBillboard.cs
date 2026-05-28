using UnityEngine;

public class TestRainDropBillboard : MonoBehaviour
{
    public Transform visualRoot;
    public float lifetime = 5f;
    public float swayAmount = 0.18f;
    public float swaySpeed = 5f;

    Camera targetCamera;
    Vector3 startPosition;
    float seed;

    void Awake()
    {
        targetCamera = Camera.main;
        startPosition = transform.position;
        seed = Random.Range(0f, 1000f);
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 position = transform.position;
        position.x += Mathf.Sin(Time.time * swaySpeed + seed) * swayAmount * Time.deltaTime;
        transform.position = position;

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            Vector3 direction = visualRoot.position - targetCamera.transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                visualRoot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }
    }
}
