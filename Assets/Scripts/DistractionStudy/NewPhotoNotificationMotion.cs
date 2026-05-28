using UnityEngine;

public class NewPhotoNotificationMotion : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public float visibleDuration = 2.2f;
    public float slideDistance = 92f;
    public float easeDuration = 0.28f;

    Vector2 restingPosition;
    float age;

    void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform != null)
        {
            restingPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = restingPosition + Vector2.up * slideDistance;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        age += Time.deltaTime;
        float inAmount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / easeDuration));
        float outAmount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - visibleDuration) / easeDuration));
        float visibleAmount = Mathf.Clamp01(inAmount - outAmount);

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = restingPosition + Vector2.up * slideDistance * (1f - inAmount);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visibleAmount;
        }

        if (age > visibleDuration + easeDuration + 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
