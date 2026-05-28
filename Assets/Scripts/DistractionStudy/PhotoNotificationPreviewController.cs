using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoNotificationPreviewController : MonoBehaviour
{
    [Header("Notification")]
    public GameObject notificationPrefab;
    public RectTransform notificationRoot;
    public Sprite instagramIcon;
    public Sprite[] notificationPhotos;
    public float firstNotificationDelay = 1f;
    public float notificationInterval = 3.2f;

    [Header("Preview")]
    public KeyCode previewKey = KeyCode.N;
    public CanvasGroup dimmerGroup;
    public CanvasGroup previewGroup;
    public RectTransform previewCard;
    public Image previewImage;
    public float previewFadeSpeed = 7f;
    public float previewScaleSpeed = 9f;
    public float externalPreviewSeconds = 0.9f;

    [Header("Stats")]
    public float experienceDuration = 30f;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI viewCountText;

    [Header("Responsive Layout")]
    public Camera layoutCamera;
    public RectTransform readingCanvas;
    public float cameraPadding = 1.08f;

    float notificationTimer;
    float previewAmount;
    float experienceTimer;
    float externalPreviewUntil;
    int viewCount;
    int notificationCount;
    bool wasPreviewing;
    int nextPhotoIndex;
    int lastScreenWidth = -1;
    int lastScreenHeight = -1;
    float lastCameraAspect = -1f;

    public int ViewCount
    {
        get { return viewCount; }
    }

    public int NotificationCount
    {
        get { return notificationCount; }
    }

    readonly string[] senders =
    {
        "Instagram",
        "studygram",
        "campus.life",
        "librarydesk",
        "focusbreak"
    };

    readonly string[] titles =
    {
        "New photo",
        "Photo from your friend",
        "Someone posted",
        "New story photo",
        "Fresh update"
    };

    readonly string[] bodies =
    {
        "A new library snapshot is waiting.",
        "Your classmate shared a desk setup.",
        "A study post just landed.",
        "See what changed in the feed.",
        "A photo from campus is ready."
    };

    void Start()
    {
        ResolveResponsiveReferences();
        ApplyResponsiveLayout(true);

        notificationTimer = -firstNotificationDelay;
        experienceTimer = experienceDuration;
        SetGroup(dimmerGroup, 0f);
        SetGroup(previewGroup, 0f);
        UpdateStats();

        if (previewCard != null)
        {
            previewCard.localScale = Vector3.one * 0.94f;
        }
    }

    void Update()
    {
        ApplyResponsiveLayout(false);

        experienceTimer = Mathf.Max(0f, experienceTimer - Time.deltaTime);
        UpdateStats();

        notificationTimer += Time.deltaTime;
        if (notificationTimer >= notificationInterval)
        {
            notificationTimer = 0f;
            SpawnNotification();
        }

        bool wantsPreview = Input.GetKey(previewKey) || Time.time < externalPreviewUntil;
        if (wantsPreview && !wasPreviewing)
        {
            viewCount++;
            UpdatePreviewPhoto(GetNextPhoto());
            UpdateStats();
        }

        wasPreviewing = wantsPreview;

        float target = wantsPreview ? 1f : 0f;
        previewAmount = Mathf.MoveTowards(previewAmount, target, previewFadeSpeed * Time.deltaTime);

        SetGroup(dimmerGroup, previewAmount);
        SetGroup(previewGroup, previewAmount);

        if (previewCard != null)
        {
            float scale = Mathf.Lerp(0.94f, 1f, Mathf.SmoothStep(0f, 1f, previewAmount));
            previewCard.localScale = Vector3.MoveTowards(previewCard.localScale, Vector3.one * scale, previewScaleSpeed * Time.deltaTime);
        }
    }

    public void PreviewForSeconds()
    {
        externalPreviewUntil = Time.time + externalPreviewSeconds;
    }

    void SpawnNotification()
    {
        if (notificationPrefab == null || notificationRoot == null)
        {
            return;
        }

        GameObject notification = Instantiate(notificationPrefab, notificationRoot);
        notification.transform.SetAsLastSibling();
        ApplyNotificationContent(notification, GetNextPhoto());
        notificationCount++;
    }

    void ApplyNotificationContent(GameObject notification, Sprite photo)
    {
        SetImage(notification.transform, "Instagram Icon", instagramIcon);
        SetText(notification.transform, "App Name", Pick(senders));
        SetText(notification.transform, "Message", Pick(titles));
        SetText(notification.transform, "Body", Pick(bodies));
        SetText(notification.transform, "Time", "now");
    }

    Sprite GetNextPhoto()
    {
        if (notificationPhotos == null || notificationPhotos.Length == 0)
        {
            return null;
        }

        Sprite photo = notificationPhotos[nextPhotoIndex % notificationPhotos.Length];
        nextPhotoIndex++;
        return photo;
    }

    void UpdatePreviewPhoto(Sprite photo)
    {
        if (previewImage != null && photo != null)
        {
            previewImage.sprite = photo;
            previewImage.preserveAspect = true;
        }
    }

    void UpdateStats()
    {
        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(experienceTimer).ToString("00") + "s";
        }

        if (viewCountText != null)
        {
            viewCountText.text = viewCount.ToString();
        }
    }

    static string Pick(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return string.Empty;
        }

        return values[Random.Range(0, values.Length)];
    }

    static void SetImage(Transform root, string childName, Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        Transform child = root.Find(childName);
        if (child == null)
        {
            return;
        }

        Image image = child.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.preserveAspect = true;
        }
    }

    static void SetText(Transform root, string childName, string value)
    {
        Transform child = root.Find(childName);
        if (child == null)
        {
            return;
        }

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = value;
        }
    }

    static void SetGroup(CanvasGroup group, float amount)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = amount;
        group.blocksRaycasts = amount > 0.01f;
        group.interactable = amount > 0.99f;
    }

    void ResolveResponsiveReferences()
    {
        if (layoutCamera == null)
        {
            layoutCamera = Camera.main;
        }

        if (readingCanvas == null)
        {
            readingCanvas = GetComponent<RectTransform>();
        }
    }

    void ApplyResponsiveLayout(bool force)
    {
        ResolveResponsiveReferences();

        if (layoutCamera == null || readingCanvas == null || !layoutCamera.orthographic)
        {
            return;
        }

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        float aspect = Mathf.Max(0.01f, layoutCamera.aspect);

        if (!force
            && screenWidth == lastScreenWidth
            && screenHeight == lastScreenHeight
            && Mathf.Approximately(aspect, lastCameraAspect))
        {
            return;
        }

        lastScreenWidth = screenWidth;
        lastScreenHeight = screenHeight;
        lastCameraAspect = aspect;

        Vector3 scale = readingCanvas.lossyScale;
        float halfCanvasWidth = readingCanvas.rect.width * Mathf.Abs(scale.x) * 0.5f;
        float halfCanvasHeight = readingCanvas.rect.height * Mathf.Abs(scale.y) * 0.5f;
        float fittedSize = Mathf.Max(halfCanvasHeight, halfCanvasWidth / aspect);

        layoutCamera.orthographicSize = fittedSize * Mathf.Max(1f, cameraPadding);
    }
}
