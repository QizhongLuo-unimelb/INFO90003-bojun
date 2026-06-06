using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StudyArticleScroller : MonoBehaviour
{
    [Header("Reading Flow")]
    public ScrollRect scrollRect;
    public float fullReadDuration = 15f;
    public float startDelay = 0f;
    public bool playOnStart = true;
    public bool syncWithSceneDuration = true;

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
        float readDuration = GetReadDuration();
        float readableElapsed = Mathf.Max(0f, elapsed - startDelay);
        float progress = Mathf.Clamp01(readableElapsed / Mathf.Max(0.01f, readDuration));
        SetScrollPosition(Mathf.Lerp(1f, 0f, progress));

        if (!hasCompleted && progress >= 1f)
        {
            hasCompleted = true;
            PhotoNotificationPreviewController preview = FindFirstObjectByType<PhotoNotificationPreviewController>();
            GameRunState.SaveInsStats(
                preview != null ? preview.ViewCount : 0,
                preview != null ? preview.NotificationCount : 0);
        }
    }

    float GetReadDuration()
    {
        if (!syncWithSceneDuration)
        {
            return fullReadDuration;
        }

        return GameSceneFlowController.GetBranchDuration(SceneManager.GetActiveScene().name);
    }

    void SetScrollPosition(float value)
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = value;
        }
    }
}
