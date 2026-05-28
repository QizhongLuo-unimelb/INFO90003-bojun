using UnityEngine;

public class TestEmailSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject emailPrefab;

    [Header("Spawn Volume")]
    public Vector3 spawnZoneSize = new Vector3(7f, 1.2f, 5f);
    public Vector3 initialVelocity = new Vector3(0f, -1.5f, 0f);

    [Header("Timing")]
    public float initialSpawnInterval = 0.32f;
    public float minimumSpawnInterval = 0.12f;
    public float accelerationRate = 0.985f;
    public int maxSpawnedEmails = 320;

    bool isSpawning;
    float timer;
    float currentInterval;
    int spawnedCount;

    void Awake()
    {
        currentInterval = initialSpawnInterval;
        timer = currentInterval;
    }

    void Update()
    {
        if (!isSpawning || emailPrefab == null || spawnedCount >= maxSpawnedEmails)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            return;
        }

        SpawnEmail();
        currentInterval = Mathf.Max(minimumSpawnInterval, currentInterval * accelerationRate);
        timer = currentInterval;
    }

    public void SetSpawning(bool enabled)
    {
        isSpawning = enabled;
    }

    public void ResetSpawnRate()
    {
        currentInterval = initialSpawnInterval;
        timer = currentInterval;
        spawnedCount = 0;
    }

    void SpawnEmail()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnZoneSize.x * 0.5f, spawnZoneSize.x * 0.5f),
            Random.Range(-spawnZoneSize.y * 0.5f, spawnZoneSize.y * 0.5f),
            Random.Range(-spawnZoneSize.z * 0.5f, spawnZoneSize.z * 0.5f));

        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-25f, 25f),
            Random.Range(0f, 360f),
            Random.Range(-18f, 18f));

        GameObject email = Instantiate(emailPrefab, transform.position + randomOffset, randomRotation);
        Rigidbody body = email.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = initialVelocity;
            body.angularVelocity = Random.insideUnitSphere * 2.5f;
        }

        spawnedCount++;
    }
}
