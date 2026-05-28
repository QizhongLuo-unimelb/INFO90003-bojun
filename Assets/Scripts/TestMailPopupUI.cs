using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMailPopupUI : MonoBehaviour
{
    public static TestMailPopupUI Instance { get; private set; }

    [Header("Template")]
    public RectTransform popupTemplate;

    [Header("Timing")]
    public float visibleSeconds = 3f;
    public float minimumInterval = 0.28f;
    public int maxVisiblePopups = 5;

    [Header("Placement")]
    public Vector2 horizontalRange = new Vector2(-520f, 520f);
    public Vector2 verticalRange = new Vector2(-260f, 280f);

    [Header("Mail Card Content")]
    public string[] senderPool =
    {
        "Reddit",
        "Canvas",
        "Subject Admin",
        "Project Team",
        "Library Services",
        "Course Coordinator",
        "Student Portal"
    };

    public string[] previewPool =
    {
        "Take action now to avoid losing access...",
        "Less than 3 months remaining...",
        "Please review this message before it expires...",
        "Your account requires attention...",
        "Open Outlook to review the latest update..."
    };

    [Header("Audio")]
    public AudioClip notificationSound;
    public float notificationVolume = 0.7f;

    readonly List<MailPopupRecord> activePopups = new List<MailPopupRecord>();
    float nextAllowedTime;
    AudioSource audioSource;
    static Sprite avatarCircleSprite;

    public int TotalSpawned { get; private set; }
    public int UnreadCount { get; private set; }
    public int ReadCount { get; private set; }
    public int ActiveCount => activePopups.Count;

    class MailPopupRecord
    {
        public RectTransform popup;
        public Coroutine fadeRoutine;
        public bool resolved;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (notificationSound == null)
        {
            notificationSound = Resources.Load<AudioClip>("Audio/OutlookLikeNotification");
        }

        if (popupTemplate != null)
        {
            popupTemplate.gameObject.SetActive(false);
            ApplyMailCardStyle(popupTemplate, "Less than 3 months rem...", "The University of Melbourne", previewPool[0]);
        }

        if (GetComponent<TestMailInteractionController>() == null)
        {
            gameObject.AddComponent<TestMailInteractionController>();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowSubject(string subject)
    {
        if (popupTemplate == null || Time.time < nextAllowedTime)
        {
            return;
        }

        nextAllowedTime = Time.time + minimumInterval;

        while (activePopups.Count >= maxVisiblePopups)
        {
            ResolveAsUnread(activePopups[0]);
        }

        RectTransform popup = Instantiate(popupTemplate, popupTemplate.parent);
        popup.gameObject.SetActive(true);
        popup.name = "MailMessagePopup";
        popup.anchoredPosition = new Vector2(
            Random.Range(horizontalRange.x, horizontalRange.y),
            Random.Range(verticalRange.x, verticalRange.y));

        string sender = Pick(senderPool, "Reddit");
        string preview = Pick(previewPool, "Open this email for more details.");
        ApplyMailCardStyle(popup, subject, sender, preview);
        PlayNotificationSound();

        MailPopupRecord record = new MailPopupRecord { popup = popup };
        activePopups.Add(record);
        TotalSpawned++;
        record.fadeRoutine = StartCoroutine(FadeAndDestroy(record));
    }

    public int MarkVisibleAsRead()
    {
        int readNow = activePopups.Count;
        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            ResolveAsRead(activePopups[i]);
        }

        return readNow;
    }

    System.Collections.IEnumerator FadeAndDestroy(MailPopupRecord record)
    {
        CanvasGroup canvasGroup = record.popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = record.popup.gameObject.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        while (elapsed < visibleSeconds)
        {
            elapsed += Time.deltaTime;
            float fadeStart = Mathf.Max(0f, visibleSeconds - 0.7f);
            canvasGroup.alpha = elapsed < fadeStart ? 1f : 1f - Mathf.InverseLerp(fadeStart, visibleSeconds, elapsed);
            yield return null;
        }

        record.fadeRoutine = null;
        ResolveAsUnread(record);
    }

    void ResolveAsUnread(MailPopupRecord record)
    {
        if (record == null || record.resolved)
        {
            return;
        }

        record.resolved = true;
        UnreadCount++;
        RemoveRecord(record);
    }

    void ResolveAsRead(MailPopupRecord record)
    {
        if (record == null || record.resolved)
        {
            return;
        }

        record.resolved = true;
        ReadCount++;
        RemoveRecord(record);
    }

    void RemoveRecord(MailPopupRecord record)
    {
        activePopups.Remove(record);
        if (record.fadeRoutine != null)
        {
            StopCoroutine(record.fadeRoutine);
        }

        if (record.popup != null)
        {
            Destroy(record.popup.gameObject);
        }
    }

    void ApplyMailCardStyle(RectTransform popup, string subject, string sender, string preview)
    {
        popup.sizeDelta = new Vector2(660f, 154f);

        Image background = popup.GetComponent<Image>();
        if (background != null)
        {
            background.color = new Color(0.99f, 0.99f, 0.99f, 0.97f);
            background.raycastTarget = false;
        }

        SetStripe(popup, "TopPixelLine", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -1.5f), new Vector2(0f, 1.5f), new Color(0.78f, 0.80f, 0.82f, 0.95f));
        SetStripe(popup, "LeftPixelLine", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f), Color.clear);
        SetStripe(popup, "DividerLine", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1.5f), new Vector2(0f, 1.5f), new Color(0.82f, 0.84f, 0.86f, 0.75f));

        RectTransform avatarCircle = EnsureImage(popup, "AvatarCircle", new Color(0.82f, 0.55f, 0.88f, 1f));
        ConfigureRect(avatarCircle, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(86f, 0f), new Vector2(70f, 70f), new Vector2(0.5f, 0.5f));

        TMP_Text avatarInitial = EnsureText(popup, "AvatarInitial");
        ConfigureRect(avatarInitial.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(86f, -1f), new Vector2(46f, 46f), new Vector2(0.5f, 0.5f));
        ConfigureText(avatarInitial, InitialFor(sender), 28f, FontStyles.Bold, new Color(0.14f, 0.08f, 0.16f, 1f), TextAlignmentOptions.Center, TextOverflowModes.Overflow);

        TMP_Text title = GetText(popup, "PopupTitle");
        if (title != null)
        {
            ConfigureRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(134f, -30f), new Vector2(340f, 32f), new Vector2(0f, 1f));
            ConfigureText(title, sender, 25f, FontStyles.Normal, new Color(0.13f, 0.14f, 0.16f, 1f), TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);
        }

        TMP_Text subjectText = GetText(popup, "SubjectText");
        if (subjectText != null)
        {
            ConfigureRect(subjectText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(134f, -70f), new Vector2(340f, 34f), new Vector2(0f, 1f));
            ConfigureText(subjectText, subject, 25f, FontStyles.Normal, new Color(0.13f, 0.14f, 0.16f, 1f), TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);
        }

        TMP_Text previewText = EnsureText(popup, "PreviewText");
        ConfigureRect(previewText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(134f, -110f), new Vector2(455f, 30f), new Vector2(0f, 1f));
        ConfigureText(previewText, preview, 20f, FontStyles.Normal, new Color(0.36f, 0.38f, 0.42f, 1f), TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);

        TMP_Text timeText = EnsureText(popup, "TimeText");
        ConfigureRect(timeText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-82f, -72f), new Vector2(124f, 34f), new Vector2(0.5f, 1f));
        ConfigureText(timeText, "Wed 22:22", 22f, FontStyles.Normal, new Color(0.14f, 0.15f, 0.17f, 1f), TextAlignmentOptions.Right, TextOverflowModes.Ellipsis);

        HideChild(popup, "SenderBadge");
        HideChild(popup, "UnreadDot");
        HideChild(popup, "SenderInitial");
        HideChild(popup, "MailGlyph");
    }

    void SetStripe(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        RectTransform rect = EnsureImage(parent, name, color);
        ConfigureRect(rect, anchorMin, anchorMax, anchoredPosition, sizeDelta, new Vector2(0.5f, 0.5f));
    }

    RectTransform EnsureImage(RectTransform parent, string name, Color color)
    {
        Transform child = parent.Find(name);
        GameObject obj = child != null ? child.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (child == null)
        {
            rect.SetParent(parent, false);
        }

        Image image = obj.GetComponent<Image>();
        if (image == null)
        {
            image = obj.AddComponent<Image>();
        }

        TMP_Text existingText = obj.GetComponent<TMP_Text>();
        if (existingText != null)
        {
            existingText.enabled = false;
        }

        image.sprite = name == "AvatarCircle" ? GetAvatarCircleSprite() : null;
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    TMP_Text EnsureText(RectTransform parent, string name)
    {
        Transform child = parent.Find(name);
        GameObject obj = child != null ? child.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (child == null)
        {
            rect.SetParent(parent, false);
        }

        return obj.GetComponent<TMP_Text>();
    }

    TMP_Text GetText(RectTransform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.pivot = pivot;
        rect.localScale = Vector3.one;
    }

    void ConfigureText(TMP_Text text, string value, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, TextOverflowModes overflow)
    {
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.overflowMode = overflow;
        text.raycastTarget = false;
    }

    string Pick(string[] pool, string fallback)
    {
        return pool != null && pool.Length > 0 ? pool[Random.Range(0, pool.Length)] : fallback;
    }

    string InitialFor(string sender)
    {
        return string.IsNullOrWhiteSpace(sender) ? "M" : sender.Trim().Substring(0, 1).ToUpperInvariant();
    }

    void PlayNotificationSound()
    {
        if (notificationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(notificationSound, notificationVolume);
        }
    }

    static Sprite GetAvatarCircleSprite()
    {
        if (avatarCircleSprite != null)
        {
            return avatarCircleSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 1) * 0.5f;
        float feather = 1.4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((radius - distance) / feather);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        avatarCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        return avatarCircleSprite;
    }

    void HideChild(RectTransform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            child.gameObject.SetActive(false);
        }
    }
}
