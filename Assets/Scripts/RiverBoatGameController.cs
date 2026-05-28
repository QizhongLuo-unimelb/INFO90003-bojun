using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RiverBoatGameController : MonoBehaviour
{
    [Header("Scene References")]
    public Transform boat;
    public Transform goal;
    public Text statusText;
    public Text timerText;
    public RiverBoatShoreTrigger shoreTrigger;

    [Header("Route")]
    public float travelSeconds = 30f;
    public Vector3 startPosition = new Vector3(-12f, 0.65f, 0f);
    public Vector3 goalPosition = new Vector3(12f, 0.65f, 0f);

    [Header("Boat Wobble")]
    public float startSwayAmplitude = 0.15f;
    public float endSwayAmplitude = 2.8f;
    public float distractionReachSeconds = 15f;
    public float swayFrequency = 0.25f;
    public float maxRollAngle = 14f;
    public float maxYawAngle = 8f;

    [Header("Focus Input")]
    public KeyCode focusKey = KeyCode.M;
    public float focusReturnSpeed = 6f;
    public float normalSwayFollowSpeed = 4f;
    public float focusedTiltMultiplier = 0.25f;
    public float externalFocusSeconds = 0.7f;

    [Header("Finish")]
    public string returnSceneName = "Main game";
    public float returnDelay = 2f;

    float elapsed;
    float finishElapsed;
    float currentSway;
    float externalFocusUntil;
    bool finished;
    bool touchedShore;

    public bool HasFinished
    {
        get { return finished; }
    }

    public bool TouchedShore
    {
        get { return touchedShore; }
    }

    void Awake()
    {
        if (shoreTrigger != null)
        {
            shoreTrigger.controller = this;
        }
    }

    void Start()
    {
        if (boat != null)
        {
            boat.position = startPosition;
        }

        if (goal != null)
        {
            goal.position = goalPosition;
        }

        SetStatus("");
        UpdateTimer();
    }

    void Update()
    {
        if (finished)
        {
            finishElapsed += Time.deltaTime;
            return;
        }

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, travelSeconds));
        float x = Mathf.Lerp(startPosition.x, goalPosition.x, progress);

        float distractionProgress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, distractionReachSeconds));
        float amplitude = Mathf.Lerp(startSwayAmplitude, endSwayAmplitude, distractionProgress);
        float swayWave = Mathf.Sin(elapsed * swayFrequency * Mathf.PI * 2f);
        bool isFocusing = Input.GetKey(focusKey) || Time.time < externalFocusUntil;
        float targetSway = isFocusing ? 0f : swayWave * amplitude;
        float swayFollowSpeed = isFocusing ? focusReturnSpeed : normalSwayFollowSpeed;

        currentSway = Mathf.MoveTowards(currentSway, targetSway, swayFollowSpeed * Time.deltaTime);

        float tiltMultiplier = isFocusing ? focusedTiltMultiplier : 1f;
        float tiltRatio = amplitude <= 0.01f ? 0f : Mathf.Clamp(currentSway / amplitude, -1f, 1f);
        float roll = -tiltRatio * maxRollAngle * distractionProgress * tiltMultiplier;
        float yaw = Mathf.Cos(elapsed * swayFrequency * Mathf.PI * 2f) * maxYawAngle * distractionProgress * tiltMultiplier;

        if (boat != null)
        {
            boat.position = new Vector3(x, startPosition.y, startPosition.z + currentSway);
            boat.rotation = Quaternion.Euler(0f, yaw, roll);
        }

        UpdateTimer();

        if (progress >= 1f)
        {
            Finish("success");
        }
    }

    public void ShoreTouched()
    {
        if (finished)
        {
            return;
        }

        touchedShore = true;
        Finish("distraction");
    }

    public void FocusForSeconds()
    {
        externalFocusUntil = Time.time + externalFocusSeconds;
    }

    void Finish(string message)
    {
        finished = true;
        SetStatus(message);
    }

    void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }

    void UpdateTimer()
    {
        if (timerText == null)
        {
            return;
        }

        float remaining = Mathf.Max(0f, travelSeconds - elapsed);
        timerText.text = Mathf.CeilToInt(remaining) + "s";
    }
}
