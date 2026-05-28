using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StudyArticleScroller : MonoBehaviour
{
    [Header("Reading Flow")]
    public ScrollRect scrollRect;
    public float fullReadDuration = 30f;
    public float startDelay = 0f;
    public bool playOnStart = true;

    [Header("Completion")]
    public string nextSceneName = "Main game";

    float elapsed;
    bool hasCompleted;

    void OnEnable()
    {
        elapsed = 0f;
        hasCompleted = false;
        SetScrollPosition(1f);
    }

    void Update()
    {
        if (!playOnStart || scrollRect == null)
        {
            return;
        }

        elapsed += Time.deltaTime;
        float readableElapsed = Mathf.Max(0f, elapsed - startDelay);
        float progress = Mathf.Clamp01(readableElapsed / Mathf.Max(0.01f, fullReadDuration));
        SetScrollPosition(Mathf.Lerp(1f, 0f, progress));

        if (!hasCompleted && progress >= 1f)
        {
            hasCompleted = true;
            PhotoNotificationPreviewController preview = FindObjectOfType<PhotoNotificationPreviewController>();
            GameRunState.SaveInsStats(
                preview != null ? preview.ViewCount : 0,
                preview != null ? preview.NotificationCount : 0);
            GameRunState.ReturnToMainFromBranch(SceneManager.GetActiveScene().name, fullReadDuration);
        }
    }

    void SetScrollPosition(float value)
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = value;
        }
    }
}
